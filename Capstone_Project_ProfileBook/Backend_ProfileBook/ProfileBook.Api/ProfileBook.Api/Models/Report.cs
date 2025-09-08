using System;

namespace ProfileBook.Api.Models
{
    public class Report
    {
        public int ReportId { get; set; }
        public int? ReportedUserId { get; set; }
        public int? PostId { get; set; }
        public int ReportingUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public bool IsResolved { get; set; } = false;

        // ADD THESE NAVIGATION PROPERTIES
        public User? ReportedUser { get; set; }
        public Post? Post { get; set; }
        public User ReportingUser { get; set; } = default!;
    }
}