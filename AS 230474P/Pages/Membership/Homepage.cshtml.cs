using AS_230474P.Data;
using AS_230474P.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using AS_230474P.Services;

namespace AS_230474P.Pages
{
    public class HomepageModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomepageModel> _logger;
        private const int MaxFailedAttempts = 3;
        private const int LockoutDurationMinutes = 5;
        private readonly AuditLogService _auditLogService;

        public string SiteKey { get; private set; }
        public string SecretKey { get; private set; }






        public HomepageModel(ApplicationDbContext context, ILogger<HomepageModel> logger, AuditLogService auditLogService)
        {
            _context = context;
            _logger = logger;
            _auditLogService = auditLogService;
            SiteKey = Environment.GetEnvironmentVariable("RECAPTCHA_SITE_KEY");
            SecretKey = Environment.GetEnvironmentVariable("RECAPTCHA_SECRET_KEY");
        }
        
        [BindProperty]
        public LoginModel Login { get; set; } = new LoginModel();
      
        public string ErrorMessage { get; set; }





        public async Task<IActionResult> OnPostAsync()

        {
            SiteKey = Environment.GetEnvironmentVariable("RECAPTCHA_SITE_KEY");
            Console.WriteLine("Environment Variable RECAPTCHA_SITE_KEY: " + Environment.GetEnvironmentVariable("RECAPTCHA_SITE_KEY"));



            if (!ModelState.IsValid)
            {
                ErrorMessage = "Invalid input. Please try again.";
                return Page();
            }

            // Validate email format
            if (!Regex.IsMatch(Login.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ErrorMessage = "Invalid email format.";
                return Page();
            }

            // Verify Google Recaptcha
            string recaptchaToken = Request.Form["recaptchaToken"];
            if (!await ValidateRecaptcha(recaptchaToken))
            {
                ErrorMessage = "Recaptcha verification failed.";
                return Page();
            }

            var user = _context.Registrations.FirstOrDefault(u => u.Email == Login.Email);
            if (user == null)
            {
                ErrorMessage = "Invalid email or password.";
                _logger.LogWarning("Failed login attempt for email: {Email}", Login.Email);
                return Page();
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                ErrorMessage = $"Your account is locked. Try again at {user.LockoutEnd.Value.ToLocalTime():HH:mm:ss}.";
                return Page();
            }

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

            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            _context.SaveChanges();

            _logger.LogInformation("User {Email} logged in successfully.", Login.Email);

            // Generate and store session token
            string sessionToken = GenerateSessionToken();
            user.SessionToken = sessionToken;
            _context.SaveChanges();

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("SessionToken", sessionToken);

            await _auditLogService.LogActionAsync("User Logged In", "Login");
            return RedirectToPage("/Membership/SuccessPage", new { userId = user.Id });
        }

        private async Task<bool> ValidateRecaptcha(string token)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify",
                        new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("secret", SecretKey),
                            new KeyValuePair<string, string>("response", token)
                        }));

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var recaptchaResult = JsonSerializer.Deserialize<RecaptchaResponse>(jsonResponse);

                    return recaptchaResult.success && recaptchaResult.score >= 0.5;
                }
            }
            catch
            {
                return false;
            }
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
            return Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        }
    }

    public class RecaptchaResponse
    {
        public bool success { get; set; }
        public double score { get; set; }
    }

    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
