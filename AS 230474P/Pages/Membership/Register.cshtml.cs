using AS_230474P.Models;
using AS_230474P.Data; // Import the DbContext
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging; // Import ILogger
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AS_230474P.Pages.Membership
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegisterModel> _logger; // Inject ILogger

        public RegisterModel(ApplicationDbContext context, ILogger<RegisterModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public RegistrationModel Registration { get; set; }

        public void OnGet()
        {
            // Renders the registration form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LogValidationErrors();
                return Page();
            }

            // Check for duplicate email
            if (await _context.Registrations.AnyAsync(r => r.Email == Registration.Email))
            {
                ModelState.AddModelError("Registration.Email", "Email already exists. Please use a different email.");
                _logger.LogWarning("Duplicate email detected: {Email}", Registration.Email);
                return Page();
            }

            // Perform password complexity checks
            string passwordFeedback = ValidatePassword(Registration.Password);
            if (!string.IsNullOrEmpty(passwordFeedback))
            {
                ModelState.AddModelError("Registration.Password", passwordFeedback);
                _logger.LogWarning("Password complexity validation failed for {Email}: {Feedback}", Registration.Email, passwordFeedback);
                return Page();
            }

            // Encrypt NRIC (replace with a proper encryption method in production)
            string encryptedNRIC = EncryptNRIC(Registration.NRIC);

            // Save the Resume file only if provided (make it optional)
            string resumePath = null;
            if (Registration.Resume != null && Registration.Resume.Length > 0)
            {
                resumePath = await SaveResumeFileAsync(Registration.Resume);
            }

            // Create a new RegistrationModel entity to save in the database
            var newRegistration = new RegistrationModel
            {
                FirstName = Registration.FirstName,
                LastName = Registration.LastName,
                Gender = Registration.Gender,
                NRIC = encryptedNRIC,
                Email = Registration.Email,
                Password = Registration.Password, // Password should be hashed in production
                DateOfBirth = Registration.DateOfBirth,
                ResumeFilePath = resumePath, // Store file path only if resume provided
                WhoAmI = Registration.WhoAmI,
                CreatedAt = DateTime.Now // Ensure CreatedAt is populated
            };

            // Add new registration to DbContext
            _context.Registrations.Add(newRegistration);

            try
            {
                // Attempt to save changes to database
                _logger.LogInformation("Saving registration data for {FirstName} {LastName}.", Registration.FirstName, Registration.LastName);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Registration data saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving registration data.");
                ModelState.AddModelError(string.Empty, "An error occurred while saving your data. Please try again.");
                return Page();
            }

            // Redirect to a success page
            return RedirectToPage("Success");
        }

        private string EncryptNRIC(string nric)
        {
            // Example encryption logic (replace with a secure implementation)
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(nric));
        }

        private async Task<string> SaveResumeFileAsync(IFormFile resumeFile)
        {
            if (resumeFile == null || resumeFile.Length == 0)
            {
                return null;
            }

            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(resumeFile.FileName);
            var filePath = Path.Combine(uploadsDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await resumeFile.CopyToAsync(stream);
            }

            return Path.Combine("uploads", fileName).Replace("\\", "/");
        }

        private string ValidatePassword(string password)
        {
            // Check for minimum password length
            if (password.Length < 12)
            {
                return "Password must be at least 12 characters long.";
            }

            // Check for uppercase, lowercase, number, and special character
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{12,}$");
            if (!regex.IsMatch(password))
            {
                return "Password must include at least one uppercase letter, one lowercase letter, one number, and one special character.";
            }

            return null; // Password is strong
        }

        private void LogValidationErrors()
        {
            foreach (var error in ModelState)
            {
                foreach (var subError in error.Value.Errors)
                {
                    _logger.LogError("Validation Error: {0}", subError.ErrorMessage);
                }
            }

            _logger.LogError("Validation failed for the registration form.");
        }
    }
}
