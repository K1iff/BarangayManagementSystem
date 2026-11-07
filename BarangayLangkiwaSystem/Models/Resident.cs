using System;
using System.ComponentModel.DataAnnotations;

namespace BarangayLangkiwaSystem.Models
{
    public class Resident
    {
        public int ResidentId { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birthdate is required.")]
        public DateTime Birthdate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Civil Status is required.")]
        public string CivilStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid contact number format.")]
        public string ContactNo { get; set; } = string.Empty;

        public bool IsMigrant { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Purok is required.")]
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
