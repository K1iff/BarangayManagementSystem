using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarangayLangkiwaSystem.Models
{
    [Table("CertificationLog")]
    public class CertificationLog
    {
        [Key]

        [Column("cert_id")]
        public int CertId { get; set; }

        public int resident_id { get; set; }

        public int type_id { get; set; }  // link to CertificationType

        public string purpose { get; set; } = string.Empty;

        public DateTime date_issued { get; set; }

        public string status { get; set; } = "Pending"; // Approved / Disapproved

        public int issued_by { get; set; } // link to UserAccount

        public string reference_code { get; set; } = string.Empty;

        [NotMapped] // This tells the database to ignore this column
        public string? IssuedBy { get; set; }

        [NotMapped] // This tells the database to ignore this column
        public string? ReferenceCode { get; set; }
    }

}

