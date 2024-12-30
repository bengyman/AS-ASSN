using AS_230474P.Data;
using AS_230474P.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AS_230474P.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<RegistrationController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegistrationModel registration)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration data received.");
                return BadRequest(ModelState);
            }

            try
            {
                // Save the resume file if uploaded
                if (registration.Resume != null && registration.Resume.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads/resumes");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(registration.Resume.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await registration.Resume.CopyToAsync(fileStream);
                    }

                    registration.ResumeFilePath = $"/uploads/resumes/{uniqueFileName}";
                }

                // Encrypt NRIC (replace with your encryption logic)
                registration.NRIC = EncryptNRIC(registration.NRIC);

                // Hash password (replace with your hashing logic)
                registration.Password = HashPassword(registration.Password);

                // Add to database
                _context.Registrations.Add(registration);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user registered: {Email}", registration.Email);
                return Ok(new { message = "Registration successful." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration.");
                return StatusCode(500, "An error occurred while processing your registration.");
            }
        }

        private string EncryptNRIC(string nric)
        {
            // Example encryption logic (replace with secure encryption)
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(nric));
        }

        private string HashPassword(string password)
        {
            // Placeholder for password hashing (use BCrypt, Argon2, etc., in production)
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}
