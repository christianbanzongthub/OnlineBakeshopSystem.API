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
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IConfiguration _configuration;

        public NotificationsController(IPushNotificationService pushNotificationService, IConfiguration configuration)
        {
            _pushNotificationService = pushNotificationService;
            _configuration = configuration;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = await ResolveUserIdAsync();
            if (userId <= 0)
                return Unauthorized(new { message = "Unauthorized." });

            var result = await _pushNotificationService.GetMyNotificationsAsync(userId, page, pageSize);

            if (result.Status >= 400)
                return StatusCode(result.Status, result);

            return Ok(result);
        }

        [HttpPatch("{notificationId:long}/read")]
        public async Task<IActionResult> MarkAsRead(long notificationId)
        {
            var userId = await ResolveUserIdAsync();
            if (userId <= 0)
                return Unauthorized(new { message = "Unauthorized." });

            var result = await _pushNotificationService.MarkAsReadAsync(userId, notificationId);

            if (result.Status >= 400)
                return StatusCode(result.Status, result);

            return Ok(result);
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = await ResolveUserIdAsync();
            if (userId <= 0)
                return Unauthorized(new { message = "Unauthorized." });

            var result = await _pushNotificationService.MarkAllAsReadAsync(userId);

            if (result.Status >= 400)
                return StatusCode(result.Status, result);

            return Ok(result);
        }

        [HttpDelete("{notificationId:long}")]
        public async Task<IActionResult> Delete(long notificationId)
        {
            var userId = await ResolveUserIdAsync();
            if (userId <= 0)
                return Unauthorized(new { message = "Unauthorized." });

            var result = await _pushNotificationService.DeleteForUserAsync(userId, notificationId);

            if (result.Status >= 400)
                return StatusCode(result.Status, result);

            return Ok(result);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendFromUser(
            [FromBody] SendUserNotificationRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = await ResolveUserIdAsync();
            if (userId <= 0)
                return Unauthorized(new { message = "Unauthorized." });

            var result = await _pushNotificationService.SendFromUserAsync(userId, request, cancellationToken);

            if (result.Status >= 400)
                return StatusCode(result.Status, result);

            return Ok(result);
        }

        private async Task<int> ResolveUserIdAsync()
        {
            var userIdClaim =
                User.FindFirstValue("userId") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdClaim, out var parsedUserId) && parsedUserId > 0)
                return parsedUserId;

            var email =
                User.FindFirstValue(ClaimTypes.Email) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(email))
                return 0;

            using var conn = new SqlConnection(_configuration["ConnectionStrings:OnlineBakeshopdb"]);
            var user = await conn.QueryFirstOrDefaultAsync(
                "SP_ONLINEBAKESHOPDB_GETUSERLOGIN",
                new { email },
                commandType: CommandType.StoredProcedure);

            if (user == null)
                return 0;

            try { return (int)user.userId; }
            catch { return 0; }
        }

        [HttpGet("{notificationId:long}")]
        public async Task<IActionResult> GetMyNotificationById(long notificationId)
        {
            var userId = await ResolveUserIdAsync();
            if (userId <= 0)
                return Unauthorized(new { message = "Unauthorized." });

            var result = await _pushNotificationService.GetMyNotificationByIdAsync(userId, notificationId);

            if (result.Status >= 400)
                return StatusCode(result.Status, result);

            return Ok(result);
        }
    }
}