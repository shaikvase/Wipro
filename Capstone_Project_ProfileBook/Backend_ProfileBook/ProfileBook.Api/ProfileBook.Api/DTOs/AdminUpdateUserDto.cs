namespace ProfileBook.Api.DTOs
{
    // DTO for an admin updating a user's details
    public class AdminUpdateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }
}