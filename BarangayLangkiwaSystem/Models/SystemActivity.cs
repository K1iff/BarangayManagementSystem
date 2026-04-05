using System;

namespace BarangayLangkiwaSystem.Models
{
    public class SystemActivity
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-info-circle";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}