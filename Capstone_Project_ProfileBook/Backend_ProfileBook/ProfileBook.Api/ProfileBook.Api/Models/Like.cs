namespace ProfileBook.Api.Models
{
    public class Like
    {
        public int LikeId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }

        // Navigation properties
        public Post Post { get; set; } = default!;
        public User User { get; set; } = default!;
    }
}