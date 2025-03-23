using FFMpegCore.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object.Video;
using Smort_api.Object.Videos;
using System.Security.Claims;
using Tiktok_api.BackgroundServices;
using Tiktok_api.Settings_Api;

namespace Tiktok_api.Controllers.Videos
{
    [ApiController]
    public partial class Videos : ControllerBase
    {
        private readonly ILogger _logger;

        private readonly ProcessVideoServices _videoProcessor;

        private ChunkHandler chunkHandler;

        public Videos(ILogger<Videos> logger, ProcessVideoServices VideoProcessor)
        {
            _logger = logger;
            _videoProcessor = VideoProcessor;
            chunkHandler = new ChunkHandler("./TempVideos", "./Videos", "mkv");
        }

        /// <summary>
        /// Receives a byte array and saves it as a video
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("Videos/UploadVideo")]
        [HttpPost]
        public async Task<IActionResult> UploadVideoAsync(VideoUploadData? Data)
        {

            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            if (Data == null || Data.MediaData == null || Data.ChunkNumber == null || Data.TotalChunks == null)
                return BadRequest("Missing data");


            Data.FileName = $"{Data.GUIDObjSender}-${Data.ChunkNumber}";

            string id = User.FindFirstValue("Id");

            chunkHandler.SaveFileChunk(Data.MediaData, Data.FileName);

            if (chunkHandler.AreAllChunksIn($"{Data.GUIDObjSender}-$", (int)(Data.TotalChunks - 1)))
            {
                byte[] videoBytes = new byte[0];

                for (int i = 0; i < Data.TotalChunks; i++)
                {
                    var tempFileName = $"{Data.GUIDObjSender}-${i}";
                    videoBytes = videoBytes.Concat(chunkHandler.GetChunkFileData(tempFileName)).ToArray();
                }

                Guid videoSavedId = Guid.NewGuid();
                Data.FileName = videoSavedId.ToString();

                chunkHandler.SaveFileChunk(videoBytes, Data.FileName+"TS");

                string input = chunkHandler.GetPathContentTemp(Data.FileName + "TS");
                string output = chunkHandler.GetPath(Data.FileName, id);


                _videoProcessor.AddToQueue(new VideoToProcessObject
                {
                    Output = output,
                    Input = input,
                    Description = Data.Description,
                    UserId = id,
                    FileName = Data.FileName
                });

                Array.Clear(videoBytes);

                chunkHandler.TempFileCleanup($"{Data.GUIDObjSender}-$", (int)(Data.TotalChunks - 1));

                videoBytes = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return Ok("Saved the new Post");
            }
            else
            {
                Data = null;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return Ok("Chunk Saved");
        }


        /// <summary>
        /// Needs a video Id and removes a video from the database and from the file structure
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        [Authorize]
        [Route("Video/DeleteVideo")]
        [HttpDelete]
        public Task<ActionResult> DeleteVideo(int videoId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult<ActionResult>(BadRequest());

            string id = User.FindFirstValue("Id");

            using MySqlCommand SelectVideoPath = new MySqlCommand();

            SelectVideoPath.CommandText =
                "SELECT File_Location FROM File WHERE Id IN " +
                "((SELECT File_Id FROM Content WHERE Id = @VideoId) UNION (SELECT Thumbnail FROM Content WHERE Id = @VideoId)); " +
                "DELETE FROM Content WHERE Id = @VideoId AND User_Id = @UserId; " +
                "DELETE FROM File WHERE Id IN (SELECT File_Id FROM Video WHERE Id = @VideoId UNION SELECT Thumbnail FROM Content WHERE Id = @VideoId);";

            SelectVideoPath.Parameters.AddWithValue("@VideoId", videoId);
            SelectVideoPath.Parameters.AddWithValue("@UserId", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                string json = databaseHandler.Select(SelectVideoPath);
                _logger.LogInformation(json);

                FilePathData[] paths = JsonConvert.DeserializeObject<FilePathData[]>(json)!;
                if (paths.Length != 1)
                    return Task.FromResult<ActionResult>(BadRequest());

                foreach (FilePathData path in paths)
                {
                    System.IO.File.Delete(path.File_Location!);
                }

            }

            return Task.FromResult<ActionResult>(Ok());
        }

        /// <summary>
        /// Streams videos to an html element or makes it so that you can download the video
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        [Route("Video/GetVideo")]
        [HttpGet]
        public ActionResult? GetVideos(int videoId, Sizes size = Sizes.M)
        {
            using MySqlCommand GetVideoPath = new MySqlCommand();

            GetVideoPath.CommandText = "SELECT File_Location FROM File WHERE Id=(SELECT File_Id FROM Content WHERE Id=@Id);";
            GetVideoPath.Parameters.AddWithValue("@Id", $"{videoId}");

            using DatabaseHandler databaseHandler = new DatabaseHandler();

            string json = databaseHandler.Select(GetVideoPath);

            FilePathData[] path = JsonConvert.DeserializeObject<FilePathData[]>(json)!;

            var filestream = System.IO.File.OpenRead(path[0].File_Location! + $"_{size}.mp4");
            return File(filestream, contentType: "video/mkv", enableRangeProcessing: true);
        }

        /// <summary>
        /// Streams videos to an html element or makes it so that you can download the video
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        [Route("Video/GetThumbnail")]
        [HttpGet]
        public ActionResult? GetThumbnail(int videoId)
        {
            try
            {
                using MySqlCommand GetVideoPath = new MySqlCommand();

                GetVideoPath.CommandText = "SELECT Thumbnail FROM Video WHERE Id=@Id;";
                GetVideoPath.Parameters.AddWithValue("@Id", $"{videoId}");

                using DatabaseHandler databaseHandler = new DatabaseHandler();

                string json = databaseHandler.Select(GetVideoPath);

                ThumbnailData[] path = JsonConvert.DeserializeObject<ThumbnailData[]>(json)!;

                return File(path[0].Thumbnail, contentType: "Image/png", enableRangeProcessing: false);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }
    }
}