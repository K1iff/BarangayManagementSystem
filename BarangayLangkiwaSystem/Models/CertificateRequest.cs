using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BarangayLangkiwaSystem.Models
{
    [Table("CertificateRequest")]
    public class CertRequest : BaseModel
    {
        [PrimaryKey("request_id", false)]
        public int RequestId { get; set; }

        [Column("resident_id")]
        public int ResidentId { get; set; }

        [Column("resident_name")]
        public string ResidentName { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        [Column("certificate_type")]
        public string CertificateType { get; set; } = string.Empty;

        [Column("purpose")]
        public string Purpose { get; set; } = string.Empty;

        [Column("request_date")]
        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Column("status")]
        public string Status { get; set; } = "Pending";
    }
}
