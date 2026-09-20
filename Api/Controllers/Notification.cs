using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tiktok_api.Services;

namespace Tiktok_api.Controllers;

[Authorize]
[ApiController]
public class Notification (INotificationServices _notificationServices) : ControllerBase
{
    [Route("Notification/RegisterFCM")]
    [HttpPost]
    public async Task<ActionResult> RegisterFcm([FromBody] string fcmToken)
    {
        var userIdClaim = User.FindFirstValue("app_user_id");
        
        if (Int32.TryParse(userIdClaim, out var userId))
        {
            await _notificationServices.RegisterFcm(userId, fcmToken);
            return Ok();
        }

        return BadRequest();
    }
}