using System.ComponentModel.DataAnnotations;

namespace BarangayLangkiwaSystem.Models
{
    public class UserAccounts
    {
        public int user_id { get; set; }

        public string username { get; set; } = string.Empty;

        public string password { get; set; } = string.Empty;

        public string staff_name { get; set; } = string.Empty;

        public string role { get; set; } = string.Empty;

        public bool is_active { get; set; } = true;
    }
}
