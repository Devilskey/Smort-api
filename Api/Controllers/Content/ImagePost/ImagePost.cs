using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smort_api.Handlers;
using Smort_api.Object.ImagePosts;
using System.Security.Claims;
using Tiktok_api.Services;

namespace Tiktok_api.Controllers.Content.ImagePost
{
    [Authorize]
    [ApiController]
    public class ImagePost : ControllerBase
    {
        private readonly IImagePostService _imagePostService;
        private readonly NotificationsHandler _notifications;

        public ImagePost(IImagePostService imagePostService, NotificationsHandler notifications)
        {
            _imagePostService = imagePostService;
            _notifications = notifications;
        }

        [HttpPost]
        [Route("ImagePosts/CreateNewPost")]
        public async Task<IActionResult> CreateNewPost(CreateNewPostData data)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string userId = User.FindFirstValue("app_user_id");
            string username = User.FindFirstValue("Username");

            try
            {
                var result = await _imagePostService.SaveImagePostAsync(userId, data);
                _notifications.SendNotificationFollowers(userId, "Notify.NewImage.Title", $"Notify.NewImage.Body:{username}");
                return Ok(result);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [Route("ImagePosts/DeleteImage")]
        [HttpDelete]
        public async Task<ActionResult> DeleteImage(int imageId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string userId = User.FindFirstValue("app_user_id");

            try
            {
                await _imagePostService.DeleteImageAsync(imageId, userId);
                return Ok();
            }
            catch (InvalidOperationException)
            {
                return BadRequest();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
