using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayLangkiwaSystem.Models
{
    [Table("UserAccount")]
    public class UserAccounts
    {
        [Key]
        public int user_id { get; set; }

        [Required]
        public string username { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;

        [Required]
        public string staff_name { get; set; } = string.Empty;

        [Required]
        public string role { get; set; } = string.Empty;

        public bool is_active { get; set; } = true;
    }
}
