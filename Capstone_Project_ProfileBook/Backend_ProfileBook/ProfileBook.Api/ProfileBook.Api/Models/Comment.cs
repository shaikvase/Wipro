namespace ProfileBook.Api.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int PostId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public Post Post { get; set; } = default!;
        public User User { get; set; } = default!;
    }
}