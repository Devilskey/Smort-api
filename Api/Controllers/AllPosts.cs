using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Smort_api.Handlers;
using System.Security.Claims;
using Tiktok_api.Controllers.Analytics;

namespace Tiktok_api.Controllers
{
    [ApiController]
    public class AllPosts : ControllerBase
    {
        private ILogger<AllPosts> _logger;
        public AllPosts(ILogger<AllPosts> logger)
        {
            _logger = logger;
        }


        [Route("Posts/GetAccountContentList")]
        [HttpGet]
        public Task<string> GetAccountContentList(int? idUser)
        {
            string? id = "";
            var IdFromToken = User.FindFirstValue("Id");

            if (idUser != null)
            {
                id = idUser.ToString();
            }else if (IdFromToken != "[null]" && IdFromToken != null)
            {
                string token = HttpContext.Request.Headers["Authorization"]!;

                if (JWTTokenHandler.IsBlacklisted(token))
                    return Task.FromResult("token is blacklisted");
                
                id = IdFromToken;
            }
            else
            {
                return Task.FromResult("No Valid User");
            }

            using MySqlCommand GetVideoPath = new MySqlCommand();
            _logger.LogInformation(id);

            GetVideoPath.CommandText =
                " SELECT Id, Title, Thumbnail, 0 AS Type  FROM Video WHERE User_Id=@Id " +
                " UNION ALL SELECT Id, Title , File_Id AS Thumbnail, 1 AS Type  FROM Image_Post WHERE User_Id=@Id; ";
            GetVideoPath.Parameters.AddWithValue("@Id", $"{id}");

            using DatabaseHandler databaseHandler = new DatabaseHandler();


            return Task.FromResult(databaseHandler.Select(GetVideoPath));
        }
    }
}
