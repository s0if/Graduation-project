using System.ComponentModel.DataAnnotations.Schema;

namespace Graduation.Model
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        [ForeignKey(nameof(User))]
        public int ReceiverId { get; set; }
        public ApplicationUser User { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
