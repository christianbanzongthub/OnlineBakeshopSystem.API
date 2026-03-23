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
        IValidationRepository validationRepository;

        public RegisterController(IRegisterRepository register, IValidationRepository validation)
        {
            registerRepository = register;
            validationRepository = validation;
        }

        [HttpPost]
        [Route("UserRegister")]
        public async Task<IActionResult> UserRegister(RegisterModel register)
        {
            var validation = await validationRepository.ValidateUser(
                register.FullName, register.Email, register.Password, register.ContactNo
            );

            if (validation.Data == null || !validation.Data.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = validation.Data?.Message ?? "Validation failed"
                });

            var response = await registerRepository.GetRegister(
                register.FullName, register.Email, register.Password,
                register.Address, register.ContactNo
            );

            return Ok(response);
        }
    }
}