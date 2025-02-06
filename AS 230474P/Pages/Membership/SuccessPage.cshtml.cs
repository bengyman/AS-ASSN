using AS_230474P.Data;
using AS_230474P.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace AS_230474P.Pages.Membership
{
    public class SuccessPageModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SuccessPageModel> _logger;
        private readonly string _encryptionKey;

        // Property to hold the user's registration data
        public RegistrationModel Registration { get; set; }

        // Property to hold error message
        public string ErrorMessage { get; set; }

        public SuccessPageModel(ApplicationDbContext context, ILogger<SuccessPageModel> logger)
        {
            _context = context;
            _logger = logger;
            DotNetEnv.Env.Load(); // Make sure this is called somewhere
            _encryptionKey = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
        }

        // OnGet method to retrieve the registration details
        public IActionResult OnGet(int userId)
        {
            // Check if session exists
            string sessionUserId = HttpContext.Session.GetString("UserId");
            string sessionToken = HttpContext.Session.GetString("SessionToken");

            if (string.IsNullOrEmpty(sessionUserId) || string.IsNullOrEmpty(sessionToken))
            {
                _logger.LogWarning("Session expired or invalid. Redirecting to login.");
                ErrorMessage = "Your session has expired. Please log in again.";
                return Page(); // Stay on the current page and display the error
            }

            // Ensure the user is accessing their own data
            if (int.Parse(sessionUserId) != userId)
            {
                _logger.LogWarning($"Unauthorized access attempt by User ID {sessionUserId} to User ID {userId}.");
                ErrorMessage = "Unauthorized access.";
                return Page(); // Stay on the current page and display the error
            }

            // Retrieve the user's registration data from the database
            Registration = _context.Registrations.FirstOrDefault(r => r.Id == userId);

            if (Registration == null)
            {
                _logger.LogError($"User with ID {userId} not found.");
                ErrorMessage = "User not found.";
                return Page(); // Stay on the current page and display the error
            }

            // Verify session token to prevent multiple logins from different devices
            if (Registration.SessionToken != sessionToken)
            {
                _logger.LogWarning($"Session token mismatch for User ID {userId}. Possible multiple login detected.");
                HttpContext.Session.Clear(); // Clear invalid session
                ErrorMessage = "You have been logged out due to logging in from another device.";
                return Page(); // Stay on the current page and display the error
            }

            try
            {
                // Decrypt NRIC and password
                Registration.NRIC = DecryptData(Registration.NRIC, _encryptionKey);
                Registration.Password = DecryptPassword(Registration.Password);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error decrypting data for user with ID {userId}: {ex.Message}");
                ErrorMessage = "Error decrypting data. Please try again later.";
                return Page(); // Stay on the current page and display the error
            }

            return Page();
        }


        private string DecryptData(string cipherText, string encryptionKey)
        {
            var parts = cipherText.Split('.');
            if (parts.Length != 2)
                throw new FormatException("Invalid encrypted data format.");

            var iv = Convert.FromBase64String(parts[0]);
            var cipherBytes = Convert.FromBase64String(parts[1]);

            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(encryptionKey);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);

            return reader.ReadToEnd();
        }

        private string DecryptPassword(string storedHash)
        {
            // For demonstration purposes, return the hash directly.
            return storedHash;
        }
    }
}
