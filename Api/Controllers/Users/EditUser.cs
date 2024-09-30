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
        /// Deletes a user. Can only delete the user whos token is being used in the auth
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("users/DeleteUser")]
        [HttpDelete]
        public Task<string> Delete()
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id");


            using MySqlCommand DeleteUserPublic = new MySqlCommand();

            DeleteUserPublic.CommandText = "DELETE FROM Users_Public WHERE Person_Id = @id;";
            DeleteUserPublic.Parameters.AddWithValue("@id", $"{id}");

            using MySqlCommand DeleteUserPrivate = new MySqlCommand();

            DeleteUserPrivate.CommandText = "DELETE FROM Users_Private WHERE Id = @id;";
            DeleteUserPrivate.Parameters.AddWithValue("@id", $"{id}");

            using MySqlCommand DeleteFollowed = new MySqlCommand();

            DeleteFollowed.CommandText = "DELETE FROM Following WHERE User_Id_Followed = @id;";
            DeleteFollowed.Parameters.AddWithValue("@id", $"{id}");

            using MySqlCommand DeleteFollower = new MySqlCommand();

            DeleteFollower.CommandText = "DELETE FROM Following WHERE User_Id_Follower = @id;";
            DeleteFollower.Parameters.AddWithValue("@id", $"{id}");

            using MySqlCommand DeleteReporter = new MySqlCommand();

            DeleteReporter.CommandText = "DELETE FROM Report_User WHERE User_Reported_Id = @id;";
            DeleteReporter.Parameters.AddWithValue("@id", $"{id}");

            using MySqlCommand DeleteReported = new MySqlCommand();

            DeleteReported.CommandText = "DELETE FROM Report_User WHERE User_Reporter_Id = @id;";
            DeleteReported.Parameters.AddWithValue("@id", $"{id}");

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                databaseHandler.EditDatabase(DeleteFollowed);
                databaseHandler.EditDatabase(DeleteFollower);
                databaseHandler.EditDatabase(DeleteReporter);
                databaseHandler.EditDatabase(DeleteReported);
                databaseHandler.EditDatabase(DeleteUserPublic);
                databaseHandler.EditDatabase(DeleteUserPrivate);

                Logger.Log(LogLevel.Information, "USER DELETE");
            }

            JWTtokenBlacklistItem jwttokenBlacklistItem = new JWTtokenBlacklistItem();

            jwttokenBlacklistItem.Token = token;
            jwttokenBlacklistItem.ExpireTime = DateTime.Now.AddHours(8);

            Logger.LogInformation(token);
            Logger.LogInformation(jwttokenBlacklistItem.Token);


            JWTTokenHandler.BlackList!.Add(jwttokenBlacklistItem);
            JWTTokenHandler.WriteBlackList();

            return Task.FromResult("User Removed");
        }

        /// <summary>
        /// changes the password of an user
        /// </summary>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        [Authorize]
        [Route("users/ChangePassword")]
        [HttpPut]
        public Task<string> ChangePassword(string newPassword)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id");

            string[] EncryptedPassword = EncryptionHandler.HashAndSaltData(newPassword);

            using MySqlCommand UpdatePassword = new MySqlCommand();

            UpdatePassword.CommandText = "UPDATE Users_Private SET Password=@Password, Salt=@Salt WHERE Id=@Id";

            UpdatePassword.Parameters.AddWithValue("@Password", EncryptedPassword[1]);
            UpdatePassword.Parameters.AddWithValue("@Salt", EncryptedPassword[0]);
            UpdatePassword.Parameters.AddWithValue("@Id", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                databaseHandler.EditDatabase(UpdatePassword);
            }

            return Task.FromResult($"Password Changed");
        }

        /// <summary>
        /// Changes the email adress of an user
        /// </summary>
        /// <param name="newEmail"></param>
        /// <returns></returns>
        [Authorize]
        [Route("users/ChangeEmail")]
        [HttpPut]
        public Task<string> ChangeEmail(string newEmail)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id");


            using MySqlCommand UpdatePassword = new MySqlCommand();

            UpdatePassword.CommandText = "UPDATE Users_Private SET Email=@Email WHERE Id=@Id";

            UpdatePassword.Parameters.AddWithValue("@Email", newEmail);
            UpdatePassword.Parameters.AddWithValue("@Id", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                databaseHandler.EditDatabase(UpdatePassword);
            }

            return Task.FromResult($"Email Updated");

        }

        /// <summary>
        /// Changes the profile picture of an user
        /// </summary>
        /// <param name="newProfilePicture"></param>
        /// <returns></returns>
        [Authorize]
        [Route("users/ChangeProfilePicture")]
        [HttpPut]
        public Task<string> ChangeProfilePicture(byte[] newProfilePicture)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id");

            using MySqlCommand UpdatePassword = new MySqlCommand();

            UpdatePassword.CommandText = "UPDATE Users_Public SET Profile_Picture=@ProfilePicture WHERE Id=@Id";

            UpdatePassword.Parameters.AddWithValue("@ProfilePicture", newProfilePicture);
            UpdatePassword.Parameters.AddWithValue("@Id", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                databaseHandler.EditDatabase(UpdatePassword);
            }

            return Task.FromResult($"Profile_Picture Updated");
        }

        /// <summary>
        /// Changes the username of an account
        /// </summary>
        /// <param name="newUsername"></param>
        /// <returns></returns>
        [Authorize]
        [Route("users/ChangeUsername")]
        [HttpPut]
        public Task<string> ChangeUsername(string newUsername)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id");


            using MySqlCommand UpdatePassword = new MySqlCommand();

            UpdatePassword.CommandText = "UPDATE Users_Public SET Username=@Username WHERE Id=@Id";

            UpdatePassword.Parameters.AddWithValue("@Username", newUsername);
            UpdatePassword.Parameters.AddWithValue("@Id", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                databaseHandler.EditDatabase(UpdatePassword);
            }

            return Task.FromResult($"Username Updated");
        }
    }
}
