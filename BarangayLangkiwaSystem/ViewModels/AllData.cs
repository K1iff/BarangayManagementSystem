using BarangayLangkiwaSystem.Models;

namespace BarangayLangkiwaSystem.ViewModels
{
    public class AllData
    {
        public List<Resident> Residents { get; set; } = new();
        public List<CertRequest> CertificateRequests { get; set; } = new();

        public int ResidentId { get; set; }
        public string? ResidentFullName { get; set; }
        public string? CertificateTypeName { get; set; }
        public string? Purpose { get; set; }
    }
}
