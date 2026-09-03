using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Smort_api.Handlers;
using System.Security.Claims;
using Dapper;

namespace Tiktok_api.Controllers.Videos
{
    public partial class Videos : ControllerBase
    {
        /// <summary>
        /// returns the data of 1 random videos
        /// </summary>
        /// <param name="videoId"></param>
        /// <returns></returns>

        [Route("Video/GetVideoFromId")]
        [HttpGet]
        public async Task<object> GetVideoFromId(int id)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            string userId = User.FindFirstValue("app_user_id");

            var sqlGetVideoFromId = "";

            if (string.IsNullOrEmpty(userId))
            {
                sqlGetVideoFromId = "SELECT Id, Title, Description, Created_At, " +
                                    "(SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Video.Id AND Reaction = \"Like\" AND Content_Type=\"vid\") AS Likes, " +
                                    " FROM Video WHERE Id=@Id;";
            }
            else
            {
                sqlGetVideoFromId = "SELECT Id, Title, Description, Created_At, " +
                                    "(SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Video.Id AND Reaction = \"Like\" AND Content_Type=\"vid\") AS Likes, " +
                                    "(SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Video.Id AND Reaction = \"Like\" AND Content_Type=\"vid\" AND User_Id=@user) AS AlreadyLiked " +
                                    " FROM Video WHERE Id=@Id;";
            }
            
            return await _db.QueryAsync<object>(sqlGetVideoFromId, new {user=userId, Id=id });
        }
    }
}
