using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;

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


        [HttpGet]
        public async Task<IActionResult> LoginStudent(@object login)
        {
            @object model = new @object();
            var response = loginRepository.GetLogin(login.Username, login.Password);
            return BadRequest();
        }
    }
}
