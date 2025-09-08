namespace ProfileBook.Api.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string ProfileImage { get; set; } = string.Empty;
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Group> Groups { get; set; } = new List<Group>();
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}