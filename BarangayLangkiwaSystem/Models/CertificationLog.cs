using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BarangayLangkiwaSystem.Models
{
    [Table("CertificationLog")]
    public class CertificationLog : BaseModel
    {
        [PrimaryKey("cert_id", false)]
        public int CertId { get; set; }

        [Column("resident_id")]
        public int resident_id { get; set; }

        [Column("type_id")]
        public int type_id { get; set; }

        [Column("purpose")]
        public string purpose { get; set; } = string.Empty;

        [Column("date_issued")]
        public DateTime date_issued { get; set; }

        [Column("status")]
        public string status { get; set; } = "Pending";

        [Column("issued_by")]
        public int issued_by { get; set; }

        [Column("reference_code")]
        public string reference_code { get; set; } = string.Empty;

        public string? IssuedBy { get; set; }

        public string? ReferenceCode { get; set; }
    }
}
