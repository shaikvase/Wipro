namespace ProfileBook.Api.Models
{
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public ICollection<User> GroupMembers { get; set; } = new List<User>();
    }
}