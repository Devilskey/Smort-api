using Microsoft.AspNetCore.Mvc;
using Smort_api.Handlers;
using System.Security.Claims;
using Tiktok_api.Services;
using Smort_api.Object.DTO;

namespace Tiktok_api.Controllers.Videos
{
    public partial class Videos : ControllerBase
    {
        [Route("Video/GetVideoFromId")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VideoDto>>> GetVideoFromId(int id)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string userId = User.FindFirstValue("app_user_id");

            try
            {
                var videos = await _videoService.GetVideoFromIdAsync(userId, id);
                return Ok(videos);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
