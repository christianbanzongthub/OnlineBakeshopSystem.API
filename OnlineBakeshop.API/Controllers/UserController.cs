using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        IUserRepository userRepository;

        public UserController(IUserRepository user)
        {
            userRepository = user;
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await userRepository.GetAllUsers();
            return Ok(response);
        }

        [HttpGet]
        [Route("GetUserById")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var response = await userRepository.GetUserById(userId);
            return Ok(response);
        }

        [HttpPut]
        [Route("UpdateUser")]
        public async Task<IActionResult> UpdateUser(UserModel user)
        {
            var response = await userRepository.UpdateUser(user);
            return Ok(response);
        }


        [HttpPost]
        [Route("UpdateProfilePicture")]
        public async Task<IActionResult> UpdateProfilePicture(
       [FromForm] int userId,
       [FromForm] IFormFile file,
       [FromServices] IWebHostEnvironment env)
        {
            if (file == null || file.Length == 0)
                return Ok(new { Status = 400, Message = "No file uploaded." });

            string[] allowed = { ".jpg", ".jpeg", ".png", ".webp" };
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (!allowed.Contains(ext))
                return Ok(new { Status = 400, Message = "Only jpg, jpeg, png, webp allowed." });

            // Save to wwwroot/images/profiles/
            string folder = Path.Combine(env.WebRootPath, "images", "profiles");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString() + ext;
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string imageUrl = $"/images/profiles/{fileName}";

            // Save URL to database
            var response = await userRepository.UpdateProfilePicture(userId, imageUrl);

            // ── Return imageUrl directly so Flutter can read it easily ──
            return Ok(new { Status = 200, Message = "Profile picture updated.", ImageUrl = imageUrl });
        }

        [HttpDelete]
        [Route("DeleteUser")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var response = await userRepository.DeleteUser(userId);
            return Ok(response);
        }
    }
}