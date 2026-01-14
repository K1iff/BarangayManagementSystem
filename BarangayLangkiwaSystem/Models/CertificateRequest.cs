using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayLangkiwaSystem.Models
{
    public class CertificateRequest
    {
        public string RequestId { get; set; } = string.Empty;
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = string.Empty;
        public string CertificateType { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Released
    }
}