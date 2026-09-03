using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object;
using Smort_api.Object.Security;
using Smort_api.Object.User;
using System.Security.Claims;
using Dapper;
using Tiktok_api.SignalRHubs;

namespace Tiktok_api.Controllers.Users
{

    public partial class Users : ControllerBase
    {

        /// <summary>
        /// follows a users his account
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("users/FollowUser")]
        [HttpPost]
        public async Task<string> FollowUser(int id)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";
            
            string idUser = User.FindFirstValue("app_user_id");
            string username = User.FindFirstValue("Username");

            Console.WriteLine(idUser);
            if (id == int.Parse(idUser))
                return $"you cannnot follow yourself";


            if (id == 0)
                return $"Failed to follow user";


            var sqlIsAlreadyFollowing = "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Follower=@UserFollower AND User_Id_Followed=@UserFollowed;";

            var checkIfAlreadyFollowing = await _db.QueryAsync<int>(sqlIsAlreadyFollowing, new { UserFollower = idUser, UserFollowed = id });
            
            var sqlFollowUserCommand = "INSERT INTO Following (User_Id_Followed, User_Id_Follower, Followed_At) VALUES (@UserFollowed, @UserFollower, @FollowedAt);";
            
            if (checkIfAlreadyFollowing.FirstOrDefault() == 0)
            {
                await _db.QueryAsync(sqlFollowUserCommand, new { UserFollower = idUser, UserFollowed = id, FollowedAt=DateTime.Now});
                return $"Now following user";
            }

            _notificationHub.SendNotificationFollowToUser(id.ToString(), $"{username} started following you");

            return $"Not able to follow this user";
        }

        /// <summary>
        /// Unfollows a users his account
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("users/UnFollowUser")]
        [HttpDelete]
        public async Task<string> UnFollowUser(int creatorId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            string id = User.FindFirstValue("app_user_id");

            if (creatorId == 0)
                return $"Failed to follow user";
            
            var sqlUnFollowUserCommand = "DELETE FROM Following WHERE User_Id_Followed=@UserFollowed AND User_Id_Follower=@UserFollower;";
            
            await _db.QueryAsync(sqlUnFollowUserCommand, new {UserFollowed=creatorId, UserFollower=id});
                
            return $"user Unfollowed";
        }

        /// <summary>
        /// Gives the followers amount of a user
        /// </summary>
        /// <returns></returns>
        [Route("users/FollowersAmount")]
        [HttpPost]
        public async Task<int?> FollowersAmount(int id)
        {
            if (id == 0)
                return null;
            
            var sqlGetFollowers = "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Followed=@UserFollowed;";

            return await _db.ExecuteScalarAsync<int>(sqlGetFollowers, new {UserFollowed=id});

        }


        /// <summary>
        /// Gets the top 5 most followed users.
        /// </summary>
        /// <returns></returns>
        [Route("Following/MostFolowers")]
        [HttpGet]
        public async Task<IEnumerable<string>>? MostFollowers(int Offset = 5)
        {
            var sqlMostFollowers = @"
                SELECT Following.User_Id_Followed, COUNT(User_Id_Follower) as Amount, Users_Public.Profile_Picture, Username
                FROM Following INNER JOIN Users_Public On Users_Public.Id = Following.User_Id_Followed
                GROUP BY User_Id_Followed ORDER BY Amount DESC LIMIT @Offset;";
            
            return await _db.QueryAsync<string>(sqlMostFollowers, new { Offset = Offset});
        }

        [Authorize]
        [Route("Following/Following")]
        [HttpGet]
        public async Task<IEnumerable<string>> Following(int Offset = 5)
        {
            string idUser = User.FindFirstValue("app_user_id");

            if (idUser == "")
                return null;
            
            var sqlMostFollowers = @"
                SELECT Following.User_Id_Followed, COUNT(User_Id_Follower) as Amount, Users_Public.Profile_Picture, Username
                FROM Following INNER JOIN Users_Public On Users_Public.Id = Following.User_Id_Followed 
                WHERE User_Id_Follower = @id
                GROUP BY User_Id_Followed ORDER BY Amount DESC LIMIT @Offset;";

            return await _db.QueryAsync<string>(sqlMostFollowers, new { Offset = Offset, id = idUser });
        }


        /// <summary>
        /// Checks if you are following the user
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("users/AlreadyFollowing")]
        [HttpPost]
        public async Task<bool?> AlreadyFollowing(int id)
        {
            string idUser = User.FindFirstValue("app_user_id");

            if (idUser == "")
                 return null;
            if (id == 0)
                return null;
            
            var sqlCheckIfFollowing = "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Followed=@UserFollowed AND User_Id_Follower=@UserFollower;";
            
            int Follow = await _db.ExecuteScalarAsync<int>(sqlCheckIfFollowing, new {UserFollowed=id, UserFollower=idUser});
            if (Follow == 0) return false;
            else return true;
        }

        /// <summary>
        /// Gives the followers amount of the user
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("users/MyFollowersAmount")]
        [HttpGet]
        public async Task<int?> MyFollowersAmount()
        {
            string id = User.FindFirstValue("app_user_id");

            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return null;

            var sqlCheckIfFollowing =  "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Followed=@UserFollowed;";
            
            return await _db.ExecuteScalarAsync<int> (sqlCheckIfFollowing, new {UserFollowed=id} );
        }
    }
}
