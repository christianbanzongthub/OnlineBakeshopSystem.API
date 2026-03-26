using Microsoft.AspNetCore.Mvc;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        [Route("UploadProductImage")]
        public async Task<IActionResult> UploadProductImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Ok(new { Status = 400, Message = "No file uploaded." });
            }

            // ALLOWED FILE TYPES
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
            string fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return Ok(new { Status = 400, Message = "Only jpg, jpeg, png, webp allowed." });
            }

            // PATH TO wwwroot/images/products/
            string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");

            // CREATE FOLDER IF IT DOES NOT EXIST YET
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // UNIQUE FILE NAME SO FILES DONT OVERWRITE EACH OTHER
            string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // SAVE FILE TO FOLDER
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // RETURN THE URL PATH BACK TO BLAZOR
            string imageUrl = $"/images/products/{uniqueFileName}";

            return Ok(new { Status = 200, ImageUrl = imageUrl });
        }
    }
}