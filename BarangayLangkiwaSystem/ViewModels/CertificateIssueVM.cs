namespace BarangayLangkiwaSystem.ViewModels
{
    public class CertificateIssueVM
    {
        public int ResidentId { get; set; }
        public string? ResidentFullName { get; set; }

        public int Age { get; set; }

        public string? CivilStatus { get; set; }

        public int TypeId { get; set; }
        public string? CertificateTypeName { get; set; }

        public string? Purpose { get; set; }
        public DateTime DateIssued { get; set; }

        public int IssuedById { get; set; }
        public string? IssuedByName { get; set; }

        public string? ReferenceCode { get; set; }

        public string? Address { get; set; }

        public int DaysofBeingaResident { get; set; }

    }
}
