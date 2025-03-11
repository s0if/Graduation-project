using Graduation.Data;
using Graduation.DTOs.Message;
using Graduation.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Graduation.Service
{
    public class ChatHub :Hub
    {
        private static readonly Dictionary<string, string> UserConnections = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections[userId] = Context.ConnectionId;
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Remove the user from the connection mapping
            var userId = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (userId != null)
            {
                UserConnections.Remove(userId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            // Send the message to the receiver
            if (UserConnections.TryGetValue(receiverId, out var receiverConnectionId))
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", senderId, message);
            }
        }
    }
}
