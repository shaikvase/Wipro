namespace ProfileBook.Api.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public int? ReceiverId { get; set; } // Make nullable for group messages
        public int? GroupId { get; set; }    // New property for group messages
        public string MessageContent { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        // Navigation properties
        public User Sender { get; set; } = default!;
        public User? Receiver { get; set; } // Nullable navigation property
        public Group? Group { get; set; }    // New navigation property
    }
}