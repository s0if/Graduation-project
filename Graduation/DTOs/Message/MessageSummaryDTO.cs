namespace Graduation.DTOs.Message
{
    public class MessageSummaryDTO
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public bool IsSender { get; set; }
    }
}
