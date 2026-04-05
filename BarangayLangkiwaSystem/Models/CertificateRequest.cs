using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayLangkiwaSystem.Models
{
    [Table("CertificateRequest")]
    public class CertRequest
    {
        [Key]
        [Column("request_id")]
        public int RequestId { get; set; }

        [Required]
        [Column("resident_id")]
        public int ResidentId { get; set; }

        [Required]
        [Column("resident_name")]
        public string ResidentName { get; set; } = string.Empty;

        // Added these as proper properties to match your Controller's logic
        [NotMapped]
        public string FirstName { get; set; } = string.Empty;

        [NotMapped]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Column("certificate_type")]
        public string CertificateType { get; set; } = string.Empty;

        [Column("purpose")]
        public string Purpose { get; set; } = string.Empty;

        [Column("request_date")]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Column("status")]
        public string Status { get; set; } = "Pending"; // Status: Pending, Approved, Released
    }
}
