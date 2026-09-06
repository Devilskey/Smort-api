using Microsoft.AspNetCore.Mvc;
using Smort_api.Handlers;
using System.Security.Claims;
using Tiktok_api.Services;

namespace Tiktok_api.Controllers.Content.Posts
{
    [ApiController]
    public partial class AllPosts : ControllerBase
    {
        private readonly ILogger<AllPosts> _logger;
        private readonly IContentService _contentService;

        public AllPosts(ILogger<AllPosts> logger, IContentService contentService)
        {
            _logger = logger;
            _contentService = contentService;
        }

        [HttpGet]
        [Route("Posts/GetContentList")]
        public async Task<IActionResult?> GetContentList(string search = "")
        {
            try
            {
                var idFromToken = User.FindFirstValue("app_user_id");
                var result = await _contentService.GetContentListAsync(idFromToken, search, 0);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("Posts/GetContentFromId")]
        public async Task<IActionResult?> GetContentFromId(int id)
        {
            try
            {
                var idFromToken = User.FindFirstValue("app_user_id");
                var result = await _contentService.GetContentFromIdAsync(idFromToken, id);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [Route("Posts/GetAccountContentList")]
        [HttpGet]
        public async Task<IActionResult> GetAccountContentList(int? idUser)
        {
            try
            {
                string? id = idUser?.ToString();
                var idFromToken = User.FindFirstValue("app_user_id");

                if (id == null && idFromToken != "[null]" && idFromToken != null)
                {
                    string token = HttpContext.Request.Headers["Authorization"]!;

                    if (JWTTokenHandler.IsBlacklisted(token))
                        return Forbid();

                    id = idFromToken;
                }

                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest();

                var result = await _contentService.GetAccountContentListAsync(id);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
