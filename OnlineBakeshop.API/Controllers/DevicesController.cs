using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model.Request;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("api/devices")]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IConfiguration _configuration;

        public DevicesController(IDeviceRepository deviceRepository, IConfiguration configuration)
        {
            _deviceRepository = deviceRepository;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDeviceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = await ResolveUserIdAsync();
            if (userId <= 0)
                return Unauthorized(new { message = "Unauthorized." });

            var result = await _deviceRepository.RegisterDeviceAsync(userId, request);

            if (result.Status >= 400)
                return StatusCode(result.Status, result);

            return Ok(result);
        }

        private async Task<int> ResolveUserIdAsync()
        {
            // 1) Preferred: direct numeric userId claim
            var userIdClaim =
                User.FindFirstValue("userId") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out var parsedUserId) && parsedUserId > 0)
                return parsedUserId;

            // 2) Backward compatibility: resolve by email from token
            var email =
                User.FindFirstValue(ClaimTypes.Email) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(email))
                return 0;

            using var conn = new SqlConnection(_configuration["ConnectionStrings:OnlineBakeshopdb"]);
            var param = new DynamicParameters();
            param.Add("@email", email);

            // Uses your existing login lookup SP to fetch userId by email
            var user = await conn.QueryFirstOrDefaultAsync(
                "SP_ONLINEBAKESHOPDB_GETUSERLOGIN",
                param,
                commandType: CommandType.StoredProcedure);

            if (user == null)
                return 0;

            try
            {
                return (int)user.userId;
            }
            catch
            {
                return 0;
            }
        }
    }
}