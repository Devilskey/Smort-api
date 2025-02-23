using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using Smort_api.Handlers;
using System.Security.Claims;
using Tiktok_api.Controllers.Analytics;

namespace Tiktok_api.Controllers.Content.Posts
{
    [ApiController]
    public partial class AllPosts : ControllerBase
    {
        private ILogger<AllPosts> _logger;
        public AllPosts(ILogger<AllPosts> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("Posts/GetContentList")]
        public IActionResult GetContentList(string search = "")
        {
            MySqlCommand sqlCommand = null;

            var IdFromToken = User.FindFirstValue("Id");

            if (IdFromToken != null)
            {
                sqlCommand = ContentHandler.GetContentAlgorithmQueryLoggedIn(IdFromToken, search);
            }
            else
            {
                sqlCommand = ContentHandler.GetContentAlgorithmQuery(search);
            }

            using (DatabaseHandler database = new())
            {
                string json = database.Select(sqlCommand);
                return Ok(json);
            }

        }



        [HttpGet]
        [Route("Posts/SearchForContent")]
        public IActionResult SearchForContent(string Search)
        {

            return Ok();
        }


        [HttpGet]
        [Route("Posts/GetContentFromId")]
        public IActionResult GetContentFromId()
        {
            return Ok();
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
            }
            else if (IdFromToken != "[null]" && IdFromToken != null)
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
                " SELECT Id, Title, Thumbnail, Type AS Type  FROM Content WHERE User_Id=@Id ";

            GetVideoPath.Parameters.AddWithValue("@Id", $"{id}");

            using DatabaseHandler databaseHandler = new DatabaseHandler();


            return Task.FromResult(databaseHandler.Select(GetVideoPath));
        }
    }
}
