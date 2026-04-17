using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        IAuthRepository authRepository;

        public AuthController(IAuthRepository auth)
        {
            authRepository = auth;
        }

        // =============================================
        // POST: Auth/RefreshToken
        // Frontend calls this when JWT expires
        // =============================================
        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenModel model)
        {
            var response = await authRepository.RefreshToken(model.RefreshToken);
            return Ok(response);
        }

        // =============================================
        // POST: Auth/RevokeToken  (Logout)
        // Frontend calls this when user logs out
        // =============================================
        [HttpPost]
        [Route("RevokeToken")]
        [Authorize]   // must be logged in to logout
        public async Task<IActionResult> RevokeToken(RevokeTokenModel model)
        {
            var response = await authRepository.RevokeToken(model.RefreshToken);
            return Ok(response);
        }
    }
}