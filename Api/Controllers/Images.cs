using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object.Videos;
using Tiktok_api.Settings_Api;
using static System.Net.Mime.MediaTypeNames;

namespace Tiktok_api.Controllers
{
    [ApiController]
    public class Images : ControllerBase
    {
        private readonly ILogger Logger;

        public Images(ILogger<Images> logger)
        {
            Logger = logger;
        }

        [Route("Images/GetUsersProfileImage")]
        [HttpGet]
        public ActionResult? GetUsersProfileImage(int UserId)
        {
            try
            {
                using DatabaseHandler databaseHandler = new DatabaseHandler();

                using MySqlCommand GetUserProfileFileId = new MySqlCommand();

                GetUserProfileFileId.CommandText = "SELECT Profile_Picture FROM Users_Public WHERE Id=@Id;";
                GetUserProfileFileId.Parameters.AddWithValue("@Id", UserId);

                var FileIdJson = databaseHandler.Select(GetUserProfileFileId);

                var id = JsonConvert.DeserializeObject<ProfilePictureFileId[]>(FileIdJson);

                if(id.Length == 0)
                {
                    return BadRequest();
                }

                Console.WriteLine(id.First().Profile_Picture);

                using MySqlCommand GetVideoPath = new MySqlCommand();

                GetVideoPath.CommandText = "SELECT File_Location FROM File WHERE Id=@Id;";
                GetVideoPath.Parameters.AddWithValue("@Id", $"{id.First().Profile_Picture}");


                string json = databaseHandler.Select(GetVideoPath);

                FilePathData[] path = JsonConvert.DeserializeObject<FilePathData[]>(json)!;

                Logger.LogInformation(path[0].File_Location);

                var filestream = System.IO.File.OpenRead(path[0].File_Location!);
                return File(filestream, contentType: "image/*", enableRangeProcessing: false);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        //WARNING THIS IS MADE WITH DUCKTAPE AND HOPE THIS WILL BREAKE DOWN SOME DAY UPDATE REQUIRED
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ImageId"></param>
        /// <returns></returns>
        [Route("Images/GetImage")]
        [HttpGet]
        public ActionResult? GetImage(int ImageId, Sizes size = Sizes.M)
        {
            try
            {
                using MySqlCommand GetVideoPath = new MySqlCommand();

                GetVideoPath.CommandText = "SELECT File_Location FROM File WHERE Id=@Id;";
                GetVideoPath.Parameters.AddWithValue("@Id", $"{ImageId}");

                using DatabaseHandler databaseHandler = new DatabaseHandler();

                string json = databaseHandler.Select(GetVideoPath);

                FilePathData[] path = JsonConvert.DeserializeObject<FilePathData[]>(json)!;
                FileStream filestream = null;

                try
                {
                    filestream = System.IO.File.OpenRead(path[0].File_Location! + $"_{size}.png");
                }catch(Exception)
                {
                    filestream = System.IO.File.OpenRead(path[0].File_Location!);
                    Console.Write("Returning old image Formate 1000X1000");
                }
                return File(filestream, contentType: "image/*", enableRangeProcessing: false);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return BadRequest();
            }
        }
    }
}
