namespace ProfileBook.Api.DTOs
{
    // DTO for an admin creating a new user
    public class AdminCreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }
}