using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object.Videos;

namespace Tiktok_api.Controllers
{
    [ApiController]
    public class Images : ControllerBase
    {
        private readonly ILogger Logger;

        public Images(ILogger<Videos> logger)
        {
            Logger = logger;
        }

        //WARNING THIS IS MADE WITH DUCKTAPE AND HOPE THIS WILL BREAKE DOWN SOME DAY UPDATE REQUIRED
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ImageId"></param>
        /// <returns></returns>
        [Route("Images/GetImage")]
        [HttpGet]
        public ActionResult? GetImage(int ImageId)
        {
            try
            {
                using MySqlCommand GetVideoPath = new MySqlCommand();

                GetVideoPath.CommandText = "SELECT File_Location FROM File WHERE Id=@Id;";
                GetVideoPath.Parameters.AddWithValue("@Id", $"{ImageId}");

                using DatabaseHandler databaseHandler = new DatabaseHandler();

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
    }
}
