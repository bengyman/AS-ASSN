using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AS_230474P.Data;
using SendGrid;
using SendGrid.Helpers.Mail;
using AS_230474P.Models;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net.Mail;

namespace AS_230474P.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ForgotPasswordModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; }
        public string Message { get; set; }
        string sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");

        public async Task<IActionResult> OnPostAsync()
        {
            var user = _context.Registrations.FirstOrDefault(u => u.Email == Email);
            if (user == null)
            {
                Message = "If an account exists, a reset link will be sent.";
                return Page();
            }

            // Generate a password reset token
            string token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour
            await _context.SaveChangesAsync();

            // Send Email
            string resetLink = Url.Page("/Membership/ResetPassword", null, new { token = token, email = Email }, Request.Scheme);
            SendEmail(Email, "Password Reset", $"Click here to reset your password: {resetLink}");
            Console.WriteLine($"Generated reset link: {resetLink}");
            Console.WriteLine($"Email: {Email}, Token: {token}, ResetLink: {resetLink}");


            Message = "A reset link has been sent to your email.";
            return Page();
        }

        private async Task SendEmail(string to, string subject, string body)
        {
            try
            {
                string apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("koolpro69@gmail.com", "As assignment");
                var toEmail = new EmailAddress(to);
                var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);
                var response = await client.SendEmailAsync(msg);

                Console.WriteLine($"Email sent: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}
