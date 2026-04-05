using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayLangkiwaSystem.Models
{
    [Table("CertificationType")]
    public class CertificationType
    {
        [Key]
        [Column("type_id")]
        public int TypeId { get; set; }

        [Required]
        [Column("type_name")]
        public string TypeName { get; set; } = string.Empty;

    
    }
}
