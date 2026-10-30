using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BarangayLangkiwaSystem.Models
{
    [Table("SystemActivity")]
    public class SystemActivity : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("user_role")]
        public string UserRole { get; set; } = string.Empty;

        [Column("icon")]
        public string Icon { get; set; } = "bi-info-circle";

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
