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
            // Retrieve the user's registration data from the database
            Registration = _context.Registrations.FirstOrDefault(r => r.Id == userId);

            if (Registration == null)
            {
                // If the user is not found, return a NotFound result
                _logger.LogError($"User with ID {userId} not found.");
                return NotFound();
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
                return StatusCode(500, "Error decrypting data");
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
