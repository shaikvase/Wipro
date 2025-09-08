namespace ProfileBook.Api.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        // "User" (default) or "Admin"
        public string? Role { get; set; } = "User";

        // Required when Role == "Admin"
        public string? InviteCode { get; set; }
    }
}
