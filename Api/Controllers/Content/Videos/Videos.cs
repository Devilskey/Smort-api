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
using Tiktok_api.Settings_Api;

namespace Tiktok_api.Controllers.Videos
{
    [ApiController]
    public partial class Videos : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ProcessVideoServices _videoProcessor;
        private readonly IDbConnection _db;
        private ChunkHandler chunkHandler;

        public Videos(ILogger<Videos> logger, ProcessVideoServices VideoProcessor, IDbConnection db)
        {
            _logger = logger;
            _videoProcessor = VideoProcessor;
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
        public async Task<IActionResult> UploadVideoAsync(VideoUploadData? Data)
        {

            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            if (Data == null || Data.MediaData == null || Data.ChunkNumber == null || Data.TotalChunks == null)
                return BadRequest("Missing data");


            Data.FileName = $"{Data.GUIDObjSender}-${Data.ChunkNumber}";

            string id = User.FindFirstValue("app_user_id");

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

                chunkHandler.SaveFileChunk(videoBytes, Data.FileName + "TS");

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
        public async Task<ActionResult> DeleteVideo(int videoId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return BadRequest();

            string id = User.FindFirstValue("app_user_id");
            
            var sqlSelectVideoPath = @"
                SELECT File_Location FROM File_Content WHERE Content_Id=@VideoId UNION 
                SELECT File_Location FROM File_Image WHERE Id=(SELECT Thumbnail FROM Content WHERE id=@VideoId);
                DELETE FROM File_Content WHERE Content_Id= @VideoId;
                DELETE FROM Content WHERE Id = @VideoId AND User_Id = @UserId;
                DELETE FROM File_Image WHERE Id In (SELECT Thumbnail FROM Content WHERE Id = @VideoId);
            ";
            
            var paths = await _db.QueryAsync<FilePathData>(sqlSelectVideoPath, new { VideoId = videoId, UserId=id });
            
            if (paths.Any())
                return BadRequest();

            foreach (FilePathData path in paths) 
            { 
                System.IO.File.Delete(path.File_Location!);
            }
            
            return Ok();
        }

        /// <summary>
        /// Streams videos to an html element or makes it so that you can download the video
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        [Route("Video/GetVideo")]
        [HttpGet]
        public async Task<ActionResult?> GetVideos(int videoId, Sizes size = Sizes.M)
        {
            var sqlGetVideoPath  = "SELECT File_Location FROM File_Content WHERE Content_Id=(SELECT Id FROM Content WHERE Id=@Id);";
            
            string json =  await _db.ExecuteScalarAsync<string>(sqlGetVideoPath, new {Id=videoId});
                
            FilePathData[] path = JsonConvert.DeserializeObject<FilePathData[]>(json)!;

            var filestream = new FileStream(path[0].File_Location! + $"_{size}.mp4", FileMode.Open, FileAccess.Read,  FileShare.Read);
            return File(filestream, contentType: "video/mp4", enableRangeProcessing: true);
        }
    }
}