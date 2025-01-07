using AS_230474P.Data;
using AS_230474P.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Linq;
using DotNetEnv;

namespace AS_230474P.Pages
{
    public class HomepageModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomepageModel> _logger;
        private readonly string _encryptionKey;

        public HomepageModel(ApplicationDbContext context, ILogger<HomepageModel> logger, string encryptionKey)
        {
            _context = context;
            _logger = logger;
            _encryptionKey = encryptionKey;

        }

        [BindProperty]
        public LoginModel Login { get; set; } = new LoginModel();

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            // Render the login form
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Invalid input. Please try again.";
                return Page();
            }

            // Verify email and password against the database
            var user = _context.Registrations.FirstOrDefault(u => u.Email == Login.Email);
            if (user == null || !VerifyPassword(Login.Password, user.Password))
            {
                ErrorMessage = "Invalid email or password.";
                _logger.LogWarning("Failed login attempt for email: {Email}", Login.Email);
                return Page();
            }

            _logger.LogInformation("User {Email} logged in successfully.", Login.Email);

            // Redirect to the success page
            return RedirectToPage("/Membership/SuccessPage", new { userId = user.Id });
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: inputPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return hashed == parts[1];
        }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
