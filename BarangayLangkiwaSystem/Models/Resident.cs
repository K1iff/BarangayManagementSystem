using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace BarangayLangkiwaSystem.Models
{
    [Table("ResidentProfile")]
    public class Resident : BaseModel
    {
        [PrimaryKey("resident_id", false)]
        public int ResidentId { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Column("middle_name")]
        public string MiddleName { get; set; } = string.Empty;

        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Column("birthdate")]
        public DateTime Birthdate { get; set; } = DateTime.Now;

        [Column("gender")]
        public string Gender { get; set; } = string.Empty;

        [Column("civil_status")]
        public string CivilStatus { get; set; } = string.Empty;

        [Column("address")]
        public string Address { get; set; } = string.Empty;

        [Column("contact_no")]
        public string ContactNo { get; set; } = string.Empty;

        [Column("is_migrant")]
        public bool IsMigrant { get; set; }

        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Column("Purok")]
        public string Purok { get; set; } = string.Empty;

        [Column("Suffix")]
        public string? Suffix { get; set; }

        [Column("Occupation")]
        public string Occupation { get; set; } = string.Empty;

        [Column("EmergencyContactName")]
        public string? EmergencyContactName { get; set; }

        [Column("EmergencyContactNo")]
        public string? EmergencyContactNo { get; set; }

        [Column("date_registered")]
        public DateTime DateRegistered { get; set; } = DateTime.Now;

        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - Birthdate.Year;
                if (Birthdate > today.AddYears(-age)) age--;
                return age;
            }
        }

        public string VoterStatus => Age >= 18 ? "Active" : "Not Voter";
    }
}
