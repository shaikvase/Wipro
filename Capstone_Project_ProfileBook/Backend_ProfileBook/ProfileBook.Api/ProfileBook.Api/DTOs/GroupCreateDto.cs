namespace ProfileBook.Api.DTOs
{
    public class GroupCreateDto
    {
        public string GroupName { get; set; } = string.Empty;
        public List<int>? MemberIds { get; set; }
    }
}