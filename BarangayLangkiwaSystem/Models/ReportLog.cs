using System;

namespace BarangayLangkiwaSystem.Models
{
    public class ReportLog
    {
        public int Id { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string ExportFormat { get; set; } = string.Empty;
        public DateTime DateGenerated { get; set; } = DateTime.Now;
    }
}
