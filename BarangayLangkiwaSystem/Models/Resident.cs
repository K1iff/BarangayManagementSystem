using System;

namespace BarangayLangkiwaSystem.Models
{
    public class Resident
    {
        public int ResidentId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public DateTime Birthdate { get; set; } = DateTime.Now;

        public string Gender { get; set; } = string.Empty;

        public string CivilStatus { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string ContactNo { get; set; } = string.Empty;

        public bool IsMigrant { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Purok { get; set; } = string.Empty;

        public string? Suffix { get; set; }

        public string Occupation { get; set; } = string.Empty;

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactNo { get; set; }

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
