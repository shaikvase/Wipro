namespace ProfileBook.Api.DTOs
{
    // A DTO to safely represent a comment
    public class CommentDto
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    // This is the main DTO for a post in the feed
    public class PostResponseDto
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string PostImage { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // --- NEW PROPERTIES ---
        public int LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
    }
}