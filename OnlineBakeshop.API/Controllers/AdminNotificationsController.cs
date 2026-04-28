using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBakeshop.API.IRepository;
using OnlineBakeshop.API.Model.Request;

namespace OnlineBakeshop.API.Controllers
{
    [ApiController]
    [Route("api/admin/notifications")]
    [Authorize(Roles = "admin")]
    public class AdminNotificationsController : ControllerBase
    {
        private readonly IPushNotificationService _pushNotificationService;

        public AdminNotificationsController(IPushNotificationService pushNotificationService)
        {
            _pushNotificationService = pushNotificationService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendAsync(
            [FromBody] SendPushNotificationRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _pushNotificationService.SendToUsersAsync(request, cancellationToken);

            if (result.Status >= 400)
                return StatusCode(result.Status, result);

            return Ok(result);
        }
    }
}