namespace BarangayLangkiwaSystem.Models
{
    public class PurokReportViewModel
    {
        public string PurokName { get; set; } = string.Empty;
        public int TotalResidents { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public int TotalHousehold { get; set; }
        public int SeniorCount { get; set; }
        public int RegisteredVoters { get; set; }
        public int NewResidents { get; set; }
    }
}