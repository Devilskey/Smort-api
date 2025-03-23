using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object;
using Smort_api.Object.Security;
using Smort_api.Object.User;
using System.Security.Claims;

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
        public Task<string> FollowUser(int id)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string idUser = User.FindFirstValue("Id");

            if (id == int.Parse(idUser))
                return Task.FromResult($"you cannnot follow yourself");


            if (id == 0)
                return Task.FromResult($"Failed to follow user");

            using MySqlCommand CheckIfAlreadyFollowing = new MySqlCommand();

            CheckIfAlreadyFollowing.CommandText = "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Follower=@UserFollower AND User_Id_Followed=@UserFollowed;";

            CheckIfAlreadyFollowing.Parameters.AddWithValue("@UserFollower", idUser);
            CheckIfAlreadyFollowing.Parameters.AddWithValue("@UserFollowed", id);

            using MySqlCommand FollowUserCommand = new MySqlCommand();

            FollowUserCommand.CommandText = "INSERT INTO Following (User_Id_Followed, User_Id_Follower, Followed_At) VALUES (@UserFollowed, @UserFollower, @FollowedAt);";

            FollowUserCommand.Parameters.AddWithValue("@UserFollowed", id);
            FollowUserCommand.Parameters.AddWithValue("@UserFollower", idUser);

            FollowUserCommand.Parameters.AddWithValue("@FollowedAt", DateTime.Now);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                if (databaseHandler.GetNumber(CheckIfAlreadyFollowing) == 0)
                {
                    databaseHandler.EditDatabase(FollowUserCommand);
                    return Task.FromResult($"Now following user");
                }
            }
            return Task.FromResult($"Not able to follow this user");
        }

        /// <summary>
        /// Unfollows a users his account
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("users/UnFollowUser")]
        [HttpDelete]
        public Task<string> UnFollowUser(int creatorId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id");

            if (creatorId == 0)
                return Task.FromResult($"Failed to follow user");

            using MySqlCommand CheckIfFollowing = new MySqlCommand();

            CheckIfFollowing.CommandText = "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Follower=@UserFollower AND User_Id_Followed=@UserFollowed;";

            CheckIfFollowing.Parameters.AddWithValue("@UserFollower", id);

            CheckIfFollowing.Parameters.AddWithValue("@UserFollowed", creatorId);

            using MySqlCommand UnFollowUserCommand = new MySqlCommand();

            UnFollowUserCommand.CommandText = "DELETE FROM Following WHERE User_Id_Followed=@UserFollowed AND User_Id_Follower=@UserFollower;";

            UnFollowUserCommand.Parameters.AddWithValue("@UserFollowed", creatorId);
            UnFollowUserCommand.Parameters.AddWithValue("@UserFollower", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                if (databaseHandler.GetNumber(CheckIfFollowing) != 0)
                {
                    databaseHandler.EditDatabase(UnFollowUserCommand);
                }
            }
            return Task.FromResult($"user Unfollowed");
        }

        /// <summary>
        /// Gives the followers amount of a user
        /// </summary>
        /// <returns></returns>
        [Route("users/FollowersAmount")]
        [HttpPost]
        public Task<int>? FollowersAmount(int id)
        {
            if (id == 0)
                return null;

            using MySqlCommand CheckIfFollowing = new MySqlCommand();

            CheckIfFollowing.CommandText = "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Followed=@UserFollowed;";

            CheckIfFollowing.Parameters.AddWithValue("@UserFollowed", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                return Task.FromResult(
                    databaseHandler.GetNumber(CheckIfFollowing)
                   );
            }
        }


        /// <summary>
        /// Gets the top 5 most followed users.
        /// </summary>
        /// <returns></returns>
        [Route("Following/MostFolowers")]
        [HttpGet]
        public string? MostFollowers(int Offset = 5)
        {

            using MySqlCommand MostFollowers = new MySqlCommand();

            MostFollowers.CommandText = @"
                SELECT Following.User_Id_Followed, COUNT(User_Id_Follower) as Amount, Users_Public.Profile_Picture, Username
                FROM Following INNER JOIN Users_Public On Users_Public.Id = Following.User_Id_Followed
                GROUP BY User_Id_Followed ORDER BY Amount DESC LIMIT @Offset;";
            MostFollowers.Parameters.AddWithValue("@Offset", Offset);
            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                 return databaseHandler.Select(MostFollowers);
            }
        }

        [Authorize]
        [Route("Following/Following")]
        [HttpGet]
        public string? Following(int Offset = 5)
        {
            string idUser = User.FindFirstValue("Id");

            if (idUser == "")
                return null;


            using MySqlCommand MostFollowers = new MySqlCommand();

            MostFollowers.CommandText = @"
                SELECT Following.User_Id_Followed, COUNT(User_Id_Follower) as Amount, Users_Public.Profile_Picture, Username
                FROM Following INNER JOIN Users_Public On Users_Public.Id = Following.User_Id_Followed 
                WHERE User_Id_Follower = @id
                GROUP BY User_Id_Followed ORDER BY Amount DESC LIMIT @Offset;";

            MostFollowers.Parameters.AddWithValue("@Offset", Offset);
            MostFollowers.Parameters.AddWithValue("@id", idUser);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                return databaseHandler.Select(MostFollowers);
            }
        }


        /// <summary>
        /// Checks if you are following the user
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("users/AlreadyFollowing")]
        [HttpPost]
        public Task<bool>? AlreadyFollowing(int id)
        {
            string idUser = User.FindFirstValue("Id");

            if (idUser == "")
                 return null;
            if (id == 0)
                return null;

            using MySqlCommand CheckIfFollowing = new MySqlCommand();

            CheckIfFollowing.CommandText = "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Followed=@UserFollowed AND User_Id_Follower=@UserFollower;";

            CheckIfFollowing.Parameters.AddWithValue("@UserFollowed", id);
            CheckIfFollowing.Parameters.AddWithValue("@UserFollower", idUser);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                int Follow = databaseHandler.GetNumber(CheckIfFollowing);

                if (Follow == 0) return Task.FromResult(false);
                else return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Gives the followers amount of the user
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("users/MyFollowersAmount")]
        [HttpGet]
        public Task<int>? MyFollowersAmount()
        {
            string id = User.FindFirstValue("Id");

            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return null;

            using MySqlCommand CheckIfFollowing = new MySqlCommand();

            CheckIfFollowing.CommandText = "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Followed=@UserFollowed;";

            CheckIfFollowing.Parameters.AddWithValue("@UserFollowed", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                return Task.FromResult(
                    databaseHandler.GetNumber(CheckIfFollowing)
                   );
            }
        }
    }
}
