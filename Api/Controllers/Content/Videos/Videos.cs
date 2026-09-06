using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object.Video;
using Smort_api.Object.Videos;
using System.Security.Claims;
using Dapper;
using Tiktok_api.BackgroundServices;
using Tiktok_api.Services;
using Tiktok_api.Settings_Api;

namespace Tiktok_api.Controllers.Videos
{
    [ApiController]
    public partial class Videos : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ProcessVideoServices _videoProcessor;
        private readonly IDbConnection _db;
        private readonly IVideoService _videoService;
        private ChunkHandler chunkHandler;

        public Videos(ILogger<Videos> logger, ProcessVideoServices VideoProcessor, IDbConnection db,
            IVideoService videoService)
        {
            _logger = logger;
            _videoProcessor = VideoProcessor;
            _videoService = videoService;
            chunkHandler = new ChunkHandler("./TempVideos", "./Videos", "mkv");
            _db = db;
        }

        /// <summary>
        /// Receives a byte array and saves it as a video
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("Videos/UploadVideo")]
        [HttpPost]
        public async Task<IActionResult> UploadVideoAsync(VideoUploadData? data)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string userId = User.FindFirstValue("app_user_id");

            try
            {
                var result = await _videoService.UploadVideoAsync(userId, data, _videoProcessor);
                return Ok(result);
            }
            catch (ArgumentException)
            {
                return BadRequest("Missing data");
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [Route("Video/GetVideo")]
        [HttpGet]
        public async Task<ActionResult?> GetVideos(int videoId, Sizes size = Sizes.M)
        {
            try
            {
                var path = await _videoService.GetVideoFilePathAsync(videoId, size);
                if (string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path))
                    return BadRequest();

                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return File(fileStream, contentType: "video/mp4", enableRangeProcessing: true);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
    }
}

