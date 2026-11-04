using System;

namespace BarangayLangkiwaSystem.Models
{
    public class CertificationLog
    {
        public int CertId { get; set; }
        public int resident_id { get; set; }
        public int type_id { get; set; }
        public string purpose { get; set; } = string.Empty;
        public DateTime date_issued { get; set; }
        public string status { get; set; } = "Pending";
        public int issued_by { get; set; }
        public string reference_code { get; set; } = string.Empty;
        public string? IssuedBy { get; set; }
        public string? ReferenceCode { get; set; }
    }
}
