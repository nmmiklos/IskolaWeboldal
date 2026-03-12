using System;

namespace VBJWeboldal.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public string? UserId { get; set; } // Null, ha vendég
        public string UserFullName { get; set; } // "Vendég" vagy az email/név
        public string ActionType { get; set; } // Pl. "GET Home/Index"
        public string Url { get; set; }
        public string? IpAddress { get; set; } // Hasznos a betolakodók ellen!
        public DateTime Timestamp { get; set; }
    }
}