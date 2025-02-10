using System;

namespace AS_230474P.Models
{
    public class AuditLog
    {
        public int Id { get; set; }  // Primary Key
        public string? UserName { get; set; } // Nullable in case of anonymous users
        public string Action { get; set; } = string.Empty; // Description of the action
        public string Page { get; set; } = string.Empty; // Page the action happened on
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Time of action
        public string IPAddress { get; set; } = string.Empty; // User IP Address
    }
}
