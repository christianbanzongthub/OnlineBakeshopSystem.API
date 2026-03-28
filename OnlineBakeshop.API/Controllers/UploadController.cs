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

            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
            string fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return Ok(new { Status = 400, Message = "Only jpg, jpeg, png, webp allowed." });
            }

            string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string imageUrl = $"/images/products/{uniqueFileName}";
            return Ok(new { Status = 200, ImageUrl = imageUrl });
        }

        
        [HttpPost]
        [Route("UploadReferenceImage")]
        public async Task<IActionResult> UploadReferenceImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Ok(new { Status = 400, Message = "No file uploaded." });
            }

            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
            string fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return Ok(new { Status = 400, Message = "Only jpg, jpeg, png, webp allowed." });
            }

            string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "referenceimages");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string imageUrl = $"/images/referenceimages/{uniqueFileName}";
            return Ok(new { Status = 200, ImageUrl = imageUrl });
        }
    }
}