using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegisterController : Controller
    {
        IRegisterRepository registerRepository;
        public RegisterController (IRegisterRepository register)
        {
            registerRepository = register;
        }


        [HttpPost]
        [Route("UserRegister")]
        public async Task<IActionResult> UserRegister(RegisterModel register)
        {
            RegisterModel model = new RegisterModel();
            var response = registerRepository.GetRegister(register.FullName,register.Email, register.Password, register.Address, register.ContactNo);
            return Ok();
        }


    }

}