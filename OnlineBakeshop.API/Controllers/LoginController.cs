using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : Controller   
    {
        ILoginRepository loginRepository;
        public LoginController(ILoginRepository login)
        {
            loginRepository = login;
        }


        [HttpPost]
        [Route("UserLogin")]
        public async Task<IActionResult> UserLogin(LoginModel login)
        {
            LoginModel model = new LoginModel();
            var response = loginRepository.GetLogin(login.Email, login.Password);
            return Ok();
        }
    }
}
