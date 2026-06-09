using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smort_api.Object.User;
using System.Security.Claims;
using Tiktok_api.SignalRHubs;
using Tiktok_api.Services;
using Newtonsoft.Json;
using Smort_api.Handlers;

namespace Tiktok_api.Controllers
{
    [ApiController]
    public class Reactions : ControllerBase
    {

        private readonly ILogger<Reactions> _logger;
        private readonly NotificationHubHandler _notificationHub;
        private readonly IReactionsService _reactionsService;

        public Reactions(ILogger<Reactions> logger, NotificationHubHandler notificationHub, IReactionsService reactionsService)
        {
            _logger = logger;
            _notificationHub = notificationHub;
            _reactionsService = reactionsService;
        }

        [HttpPost("Reactions/Like")]
        [Authorize]
        public async Task<IActionResult> Like(string contentId, string ContentType)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;


            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized();

            Console.WriteLine(ContentType);

            if (ContentType != "img" && ContentType != "vid")
            {
                return BadRequest();
            }

            string userId = User.FindFirstValue("Id");
            string username = User.FindFirstValue("Username");

            var result = await _reactionsService.ToggleLikeAsync(userId, contentId, ContentType);

            if (result.TypeOfLike == "Like" && result.Owner.HasValue)
            {
                var owner = result.Owner.Value;
                await _notificationHub.SendNotificationLikeToUser(owner.Id.ToString(), $"{username} liked your video");
            }

            return Ok(result.TypeOfLike);
        }
    }
}
