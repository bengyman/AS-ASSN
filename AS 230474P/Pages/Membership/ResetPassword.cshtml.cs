using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AS_230474P.Data;
using AS_230474P.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace AS_230474P.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ResetPasswordModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Token { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string Message { get; set; }

        public IActionResult OnGet(string token, string email)
        {
            var user = _context.Registrations.FirstOrDefault(u => u.Email == email && u.PasswordResetToken == token);

            if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            {
                Message = "Invalid or expired reset link.";
                return Page();
            }

            Email = email;
            Token = token;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (NewPassword != ConfirmPassword)
            {
                Message = "Passwords do not match.";
                return Page();
            }

            var user = _context.Registrations.FirstOrDefault(u => u.Email == Email && u.PasswordResetToken == Token);
            if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
            {
                Message = "Invalid or expired reset link.";
                return Page();
            }

            // Hash the new password
            string hashedPassword = HashPassword(NewPassword);

            // Update the user's password
            user.Password = hashedPassword;
            user.PasswordResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();
            return RedirectToPage("/Login"); // Redirect to login after reset
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }
    }
}
