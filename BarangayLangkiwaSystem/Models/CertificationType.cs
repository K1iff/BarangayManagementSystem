using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BarangayLangkiwaSystem.Models
{
    [Table("CertificationType")]
    public class CertificationType : BaseModel
    {
        [PrimaryKey("type_id", false)]
        public int TypeId { get; set; }

        [Column("type_name")]
        public string TypeName { get; set; } = string.Empty;
    }
}
