using AS_230474P.Data;
using AS_230474P.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AS_230474P.Services
{
    public class AuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActionAsync(string action, string page)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var userName = "Anonymous";

            // Check if the user is authenticated
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                // Attempt to fetch the user's name from claims
                var firstName = httpContext.User.FindFirst("FirstName")?.Value;
                var lastName = httpContext.User.FindFirst("LastName")?.Value;

                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    userName = $"{firstName} {lastName}";
                }
                else
                {
                    // If no first/last name, use another claim (e.g., username or email)
                    userName = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
                }
            }

            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var log = new AuditLog
            {
                UserName = userName,
                Action = action,
                Page = page,
                Timestamp = DateTime.UtcNow,
                IPAddress = ip
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
