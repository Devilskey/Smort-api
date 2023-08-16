using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object.Videos;
using System.Security.Claims;

namespace Tiktok_api.Controllers
{
    [ApiController]
    public class Videos : ControllerBase
    {
        private readonly ILogger Logger;

        public Videos(ILogger<Videos> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Receives a byte array and saves it as a video
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        [Authorize]
        [Route("Videos/UploadVideo")]
        [HttpPost]
        public Task<string> UploadVideo(VideoUploadData? Data)
        {
            Guid videoSavedId = Guid.NewGuid();

            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            if (Data == null)
                return Task.FromResult("Video Failed to save");

            Data.FileName = $"{videoSavedId}.mkv";

            if (Data.MediaData == null)
                return Task.FromResult("No Video");

            string id = User.FindFirstValue("Id");

            using DatabaseHandler databaseHandler = new DatabaseHandler();

            using MySqlCommand InsertFileAndVideo = new MySqlCommand();

            InsertFileAndVideo.CommandText =
                @"INSERT INTO File (File_Name, File_location, Created_At, Deleted_At) VALUES (@FileName, @FileLocation, @CreatedAt, @DeletedAt);
                  INSERT INTO Video (User_Id, File_Id, Title, Description, Created_At, Updated_At, Deleted_At) VALUES (@Id, LAST_INSERT_ID(), @Title, @Description, @CreatedAt, @UpdatedAt, @DeletedAt);";
           
            InsertFileAndVideo.Parameters.AddWithValue("@FileName", Data.FileName);
            InsertFileAndVideo.Parameters.AddWithValue("@Id", id);
            InsertFileAndVideo.Parameters.AddWithValue("@FileLocation", $"./Videos/{id}/{Data.FileName}");
            InsertFileAndVideo.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            InsertFileAndVideo.Parameters.AddWithValue("@DeletedAt", DateTime.Now);
            InsertFileAndVideo.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
            InsertFileAndVideo.Parameters.AddWithValue("@Title", Data.Title);
            InsertFileAndVideo.Parameters.AddWithValue("@Description", Data.Description);

            using MySqlCommand GetIdVideo = new MySqlCommand();

            databaseHandler.EditDatabase(InsertFileAndVideo);

            VideoFileHandler.SaveVideo(Data.MediaData, Data.FileName, id);

            return Task.FromResult("Video Saved");
        }

        /// <summary>
        /// returns  the data if 10 random videos
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        [Route("Video/GetVideoList")]
        [HttpGet]
        public Task<string> GetVideosList()
        {
            using MySqlCommand getRandomVideos = new MySqlCommand();
            getRandomVideos.CommandText = "SELECT Id, Title, Description, Created_At FROM Video ORDER BY RAND() LIMIT 5;";

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                string json = databaseHandler.Select(getRandomVideos);
                Logger.LogInformation(json);
                return Task.FromResult(json);
            }
        }

        /// <summary>
        /// returns the data of 1 random videos
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        [Route("Video/GetOneVideo")]
        [HttpGet]
        public Task<string> GetOneVideo()
        {
            using MySqlCommand getRandomVideos = new MySqlCommand();
            getRandomVideos.CommandText = "SELECT Id, Title, Description, Created_At FROM Video ORDER BY RAND() LIMIT 1;";

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                string json = databaseHandler.Select(getRandomVideos);
                Logger.LogInformation(json);
                return Task.FromResult(json);
            }
        }

        //WARNING THIS IS MADE WITH DUCKTAPE AND HOPE THIS WILL BREAKE DOWN SOME DAY UPDATE REQUIRED
        /// <summary>
        /// Streams videos to an html element or makes it so that you can download the video
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>
        [Route("Video/GetVideo")]
        [HttpGet]
        public ActionResult? GetVideos(int videoId)
        {
            try 
            {
                using MySqlCommand GetVideoPath = new MySqlCommand();

                GetVideoPath.CommandText = "SELECT File_Location FROM File WHERE Id=(SELECT File_Id FROM Video WHERE Id=@Id);";
                GetVideoPath.Parameters.AddWithValue("@Id", $"{videoId}");

                using DatabaseHandler databaseHandler = new DatabaseHandler();

                string json = databaseHandler.Select(GetVideoPath);

                VideoPathData[] path = JsonConvert.DeserializeObject<VideoPathData[]>(json)!;

                var filestream = System.IO.File.OpenRead(path[0].File_Location!);
                return File(filestream, contentType: "video/mkv", enableRangeProcessing: true);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return BadRequest();
            }
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
            SelectVideoPath.CommandText = "SELECT File_Location FROM File WHERE Id=(SELECT File_Id FROM Video WHERE Id=@VideoId);";
            SelectVideoPath.Parameters.AddWithValue("@VideoId", videoId);

            using MySqlCommand DeleteVideo = new MySqlCommand();

            DeleteVideo.CommandText = @"DELETE FROM Video WHERE Id=@VideoId AND User_Id=@UserId;
                                        DELETE FROM File WHERE Id=@VideoId;";

            DeleteVideo.Parameters.AddWithValue("@VideoId", videoId);
            DeleteVideo.Parameters.AddWithValue("@UserId", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                string json = databaseHandler.Select(SelectVideoPath);

                VideoPathData[] path = JsonConvert.DeserializeObject<VideoPathData[]>(json)!;
                if(path.Length != 1)
                    return Task.FromResult<ActionResult>(BadRequest());

                System.IO.File.Delete(path[0].File_Location!);

                databaseHandler.EditDatabase(DeleteVideo);
            }

            return Task.FromResult<ActionResult>(Ok());
        }
    }
}