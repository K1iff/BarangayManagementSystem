using System;

namespace BarangayLangkiwaSystem.Models
{
    public class CertRequest
    {
        public int RequestId { get; set; }
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CertificateType { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";
    }
}
