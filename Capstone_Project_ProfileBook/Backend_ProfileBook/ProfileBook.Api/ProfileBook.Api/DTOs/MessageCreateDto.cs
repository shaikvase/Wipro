namespace ProfileBook.Api.DTOs
{
    public class MessageCreateDto
    {
        public int? ReceiverId { get; set; }
        public int? GroupId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}