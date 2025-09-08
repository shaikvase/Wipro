namespace ProfileBook.Api.DTOs
{
    public class ReportDto
    {
        public int? ReportedUserId { get; set; }
        public int? PostId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}