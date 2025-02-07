using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace AS_230474P.Models
{
    [Table("Registrations")] // Explicitly map to the "Registrations" table
    public class RegistrationModel
    {
        [Key] // Primary Key
        public int Id { get; set; } // Auto-incremented primary key

        [Required(ErrorMessage = "First Name is required.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "NRIC is required.")]
        [Display(Name = "NRIC")]
        public string NRIC { get; set; }

        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [NotMapped] // Exclude from database
        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [NotMapped] // Exclude from database
        [Display(Name = "Resume")]
        public IFormFile Resume { get; set; }

        [Display(Name = "Resume File Path")]
        public string? ResumeFilePath { get; set; }

        [Display(Name = "Who Am I")]
        [Required(ErrorMessage = "Who Am I section is required.")]
        public string WhoAmI { get; set; }

        [Display(Name = "Created at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Default value

        [Display(Name = "Session Token")]
        public string? SessionToken { get; set; }

        [Display(Name = "Failed Login Attempts")]
        public int FailedLoginAttempts { get; set; } = 0; // Track failed attempts

        [Display(Name = "Lockout End Time")]
        public DateTime? LockoutEnd { get; set; } // Store lockout time
    }
}
