namespace ProfileBook.Api.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string PostImage { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        
        public User User { get; set; } = default!;

        // --- ADD THESE TWO LINES ---
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}