using AS_230474P.Data;
using AS_230474P.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Linq;
using DotNetEnv;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography; // Make sure to include this namespace for session handling.

namespace AS_230474P.Pages
{
    public class HomepageModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomepageModel> _logger;
        private readonly string _encryptionKey;
        private const int MaxFailedAttempts = 3; // Lock account after 3 failures
        private const int LockoutDurationMinutes = 5; // Lock duration before allowing retry

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

            var user = _context.Registrations.FirstOrDefault(u => u.Email == Login.Email);

            if (user == null)
            {
                ErrorMessage = "Invalid email or password.";
                _logger.LogWarning("Failed login attempt for email: {Email}", Login.Email);
                return Page();
            }

            // Check if the account is locked
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                ErrorMessage = $"Your account is locked. Try again at {user.LockoutEnd.Value.ToLocalTime():HH:mm:ss}.";
                return Page();
            }

            // Verify password
            if (!VerifyPassword(Login.Password, user.Password))
            {
                user.FailedLoginAttempts += 1;

                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
                    _logger.LogWarning("User {Email} is locked out until {Time}", Login.Email, user.LockoutEnd);
                    ErrorMessage = $"Too many failed attempts. Your account is locked for {LockoutDurationMinutes} minutes.";
                }
                else
                {
                    ErrorMessage = "Invalid email or password.";
                }

                _context.SaveChanges();
                return Page();
            }

            // Reset failed attempts after successful login
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            _context.SaveChanges();

            _logger.LogInformation("User {Email} logged in successfully.", Login.Email);

            // Generate a unique session token
            string sessionToken = GenerateSessionToken();

            // Store session token in database
            user.SessionToken = sessionToken;
            _context.SaveChanges();

            // Store session details in HTTP session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("SessionToken", sessionToken);

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

        private string GenerateSessionToken()
        {
            // Generate a unique session token using a secure random number generator
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
