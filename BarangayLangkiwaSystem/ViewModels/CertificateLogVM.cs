using BarangayLangkiwaSystem.Models;

namespace BarangayLangkiwaSystem.ViewModels
{
    public class CertificateLogVM
    {
        public CertificationLog? Log { get; set; }
        public string? ResidentName { get; set; }
        public string? IssuedByName { get; set; }
        public string? TypeName { get; set; }
    }
}
