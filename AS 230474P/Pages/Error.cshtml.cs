using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AS_230474P.Pages
{
    public class ErrorModel : PageModel  //  Must match Error.cshtml
    {
        [BindProperty(SupportsGet = true)]
        public int Code { get; set; } = 500; // Default to 500 if null

        public string Message { get; private set; } = "An unexpected error occurred.";

        public void OnGet()
        {
            Message = Code switch
            {
                404 => "Page not found.",
                403 => "Access denied.",
                500 => "Internal server error.",
                _ => "Something went wrong."
            };

            Response.StatusCode = Code; // Ensure correct status code is set
        }
    }
}
