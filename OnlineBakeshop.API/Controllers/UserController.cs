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

        [HttpDelete]
        [Route("DeleteUser")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var response = await userRepository.DeleteUser(userId);
            return Ok(response);
        }
    }
}