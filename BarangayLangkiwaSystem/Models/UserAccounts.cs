using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BarangayLangkiwaSystem.Models
{
    [Table("UserAccount")]
    public class UserAccounts : BaseModel
    {
        [PrimaryKey("user_id", false)]
        public int user_id { get; set; }

        [Column("username")]
        public string username { get; set; } = string.Empty;

        [Column("password")]
        public string password { get; set; } = string.Empty;

        [Column("staff_name")]
        public string staff_name { get; set; } = string.Empty;

        [Column("role")]
        public string role { get; set; } = string.Empty;

        [Column("is_active")]
        public bool is_active { get; set; } = true;
    }
}
