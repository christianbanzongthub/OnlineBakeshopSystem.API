using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : Controller
    {
        IAdminRepository adminRepository;

        public AdminController(IAdminRepository admin)
        {
            adminRepository = admin;
        }

        [HttpPost]
        [Route("AdminLogin")]
        public async Task<IActionResult> AdminLogin(AdminLoginModel admin)
        {
            var response = await adminRepository.AdminLogin(admin);
            return Ok(response);
        }

        [HttpGet]
        [Route("GetAllAdmins")]
        public async Task<IActionResult> GetAllAdmins()
        {
            var response = await adminRepository.GetAllAdmins();
            return Ok(response);
        }

        [HttpGet]
        [Route("GetAdminById")]
        public async Task<IActionResult> GetAdminById(int adminId)
        {
            var response = await adminRepository.GetAdminById(adminId);
            return Ok(response);
        }

        [HttpPost]
        [Route("CreateAdmin")]
        public async Task<IActionResult> CreateAdmin(AdminModel admin)
        {
            var response = await adminRepository.CreateAdmin(admin);
            return Ok(response);
        }

        [HttpPut]
        [Route("UpdateAdmin")]
        public async Task<IActionResult> UpdateAdmin(AdminModel admin)
        {
            var response = await adminRepository.UpdateAdmin(admin);
            return Ok(response);
        }

        [HttpDelete]
        [Route("DeleteAdmin")]
        public async Task<IActionResult> DeleteAdmin(int adminId)
        {
            var response = await adminRepository.DeleteAdmin(adminId);
            return Ok(response);
        }
    }
}