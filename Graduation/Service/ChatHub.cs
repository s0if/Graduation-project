using Graduation.Data;
using Graduation.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Claims;

namespace Graduation.Service
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> UserConnections = new();
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ChatHub> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(ApplicationDbContext dbContext, ILogger<ChatHub> logger, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections[userId] = Context.ConnectionId;
                _logger.LogInformation($"User {userId} connected with connection ID: {Context.ConnectionId}");
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections.TryRemove(userId, out _);
                _logger.LogInformation($"User {userId} disconnected");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string receiverId, string messageContent)
        {
            var senderId = GetUserId();
            if (string.IsNullOrEmpty(senderId))
                throw new HubException("User not authenticated");

            if (string.IsNullOrWhiteSpace(messageContent))
                throw new HubException("Message cannot be empty");

            if (!int.TryParse(senderId, out int senderIdInt) || !int.TryParse(receiverId, out int receiverIdInt))
                throw new HubException("Invalid user ID format");

            var receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver == null)
                throw new HubException("Receiver not found");

            var message = new ChatMessage
            {
                SenderId = senderIdInt,
                ReceiverId = receiverIdInt,
                Message = messageContent,
                Timestamp = DateTime.UtcNow
            };

            await _dbContext.Messages.AddAsync(message);
            await _dbContext.SaveChangesAsync();

            string roomName = GetRoomName(senderIdInt, receiverIdInt);
            await Clients.Group(roomName).SendAsync("ReceiveMessage", new
            {
                SenderId = senderIdInt,
                Message = messageContent,
                Timestamp = message.Timestamp
            });

            await Clients.Caller.SendAsync("MessageSent", message.Id);
        }

        public async Task JoinChatRoom(int otherUserId)
        {
            var currentUserId = GetUserId();
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int currentId))
                throw new HubException("Invalid current user ID");

            string roomName = GetRoomName(currentId, otherUserId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            _logger.LogInformation($"User {currentId} joined room {roomName}");
        }

        private string GetRoomName(int userId1, int userId2)
        {
            return userId1 < userId2 ? $"Chat_{userId1}{userId2}" : $"Chat{userId2}_{userId1}";
        }

        private string GetUserId() => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}