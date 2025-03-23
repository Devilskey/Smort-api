using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Smort_api.Handlers;

using System.Security.Claims;

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
        public Task<string> GetVideoFromId(int id)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string userId = User.FindFirstValue("Id");
            using MySqlCommand GetVideo = new MySqlCommand();


            if (string.IsNullOrEmpty(userId))
            {
                GetVideo.CommandText = "SELECT Id, Title, Description, Created_At, " +
                     "(SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Video.Id AND Reaction = \"Like\" AND Content_Type=\"vid\") AS Likes, " +
                    " FROM Video WHERE Id=@Id;";
            }
            else
            {
                GetVideo.CommandText = "SELECT Id, Title, Description, Created_At, " +
                     "(SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Video.Id AND Reaction = \"Like\" AND Content_Type=\"vid\") AS Likes, " +
                     "(SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Video.Id AND Reaction = \"Like\" AND Content_Type=\"vid\" AND User_Id=@user) AS AlreadyLiked " +
                    " FROM Video WHERE Id=@Id;";
                GetVideo.Parameters.AddWithValue("@user", userId);

            }
            GetVideo.Parameters.AddWithValue("@Id", id);
            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                string json = databaseHandler.Select(GetVideo);
                return Task.FromResult(json);
            }
        }
    }
}
