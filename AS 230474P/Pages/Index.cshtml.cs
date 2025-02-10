using AS_230474P.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AS_230474P.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly AuditLogService _auditLogService;

        public IndexModel(ILogger<IndexModel> logger, AuditLogService auditLogService)
        {
            _logger = logger;
            _auditLogService = auditLogService;
        }

        public async Task OnGet()
        {
            await _auditLogService.LogActionAsync("Visited Home Page", "Index");
        }
    }
}
