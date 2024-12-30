using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NanoidDotNet;


namespace AS_230474P.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileController(IWebHostEnvironment environment) : ControllerBase
    {
        private readonly IWebHostEnvironment _environment = environment;

        [HttpPost("upload"), Authorize]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded or file is empty.");

            // Validate file type (e.g., allow only images)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Invalid file type. Only image files are allowed.");

            // Limit file size (e.g., 5 MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("File size exceeds the 5 MB limit.");

            var id = Nanoid.Generate(size: 10);
            var filename = id + fileExtension;
            var imagePath = Path.Combine(_environment.ContentRootPath, "wwwroot/uploads", filename);

            using var fileStream = new FileStream(imagePath, FileMode.Create);
            file.CopyTo(fileStream);

            return Ok(new { filename });
        }

    }




}