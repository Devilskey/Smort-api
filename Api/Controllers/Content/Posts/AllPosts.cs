using System.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Smort_api.Handlers;
using System.Security.Claims;
using Dapper;

namespace Tiktok_api.Controllers.Content.Posts
{
    [ApiController]
    public partial class AllPosts : ControllerBase
    {
        private ILogger<AllPosts> _logger;
        private readonly IDbConnection _db;

        public AllPosts(ILogger<AllPosts> logger, IDbConnection db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        [Route("Posts/GetContentList")]
        public async Task<IActionResult?> GetContentList(string search = "")
        {
            var sqlCommand = "";

            var IdFromToken = User.FindFirstValue("app_user_id");

            if (IdFromToken != null)
            {
                sqlCommand = ContentHandler.GetContentAlgorithmQueryLoggedIn(search);
                
                return Ok(await _db.QueryAsync<object>(sqlCommand, new { @asked = search.ToLower(), max=30, offset=30, user=IdFromToken}));
            }
            else
            {
                sqlCommand = ContentHandler.GetContentAlgorithmQuery(search);
                
                return Ok(await _db.QueryAsync<object>(sqlCommand, new { @asked = search.ToLower(), max=30, offset=30}));
            }
        }

        [HttpGet]
        [Route("Posts/GetContentFromId")]
        public async Task<IActionResult?> GetContentFromId(int id)
        {
            var sqlCommand = "";

            var IdFromToken = User.FindFirstValue("app_user_id");

            if (IdFromToken != null)
            {
                sqlCommand = ContentHandler.GetContentItemAlgorithmQueryLoggedIn();
                return Ok(await _db.QueryAsync<object>(sqlCommand, new { @user = IdFromToken,  @Contentid = id.ToString() }));
            }
            else
            {
                sqlCommand = ContentHandler.GetContentItemAlgorithmQuery();
                return Ok(await _db.QueryAsync<object>(sqlCommand, new { @Contentid = id.ToString() }));
            }
        }
        
        [Route("Posts/GetAccountContentList")]
        [HttpGet]
        public async Task<object?> GetAccountContentList(int? idUser)
        {
            string? id = "";
            var IdFromToken = User.FindFirstValue("app_user_id");

            if (idUser != null)
            {
                id = idUser.ToString();
            }
            else if (IdFromToken != "[null]" && IdFromToken != null)
            {
                string token = HttpContext.Request.Headers["Authorization"]!;

                if (JWTTokenHandler.IsBlacklisted(token))
                    return Forbid();

                id = IdFromToken;
            }
            else
            {
                return BadRequest();
            }
            
            var sqlGetVideoPath =
                @"SELECT Content.Id, Content.Thumbnail, Content.Type, File_Content.Id as File_Id
                FROM Content LEFT JOIN File_Content ON  Content.Id=File_Content.Content_Id 
                WHERE User_Id=@Id ";
            
            return await _db.QueryAsync<object>(sqlGetVideoPath, new {Id=id});
        }
    }
}
