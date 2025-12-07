using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object;
using Smort_api.Object.Security;
using Smort_api.Object.User;
using Smort_api.Object.Videos;
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

            using MySqlCommand DeleteUserAndGetFilePath = new MySqlCommand();

            DeleteUserAndGetFilePath.CommandText =
                "SELECT File_location FROM File WHERE Id=(SELECT Profile_Picture FROM Users_Public WHERE Id=@id) UNION " +
                "SELECT File_location FROM File WHERE Id=(SELECT File_Id FROM Image_Post WHERE User_Id=@id) UNION " +
                "SELECT File_location FROM File WHERE Id=(SELECT File_Id FROM Video WHERE User_Id=@id) UNION " +
                "SELECT File_location FROM File WHERE Id=(SELECT Thumbnail FROM Video WHERE User_Id=@id); " +
                "DELETE FROM Users_Public WHERE Person_Id = @id; " +
                "DELETE FROM Users_Private WHERE Id = @id; " +
                "DELETE FROM Following WHERE User_Id_Followed = @id; " +
                "DELETE FROM Following WHERE User_Id_Follower = @id; " +
                "DELETE FROM Report_User WHERE User_Reported_Id = @id; " +
                "DELETE FROM Report_User WHERE User_Reporter_Id = @id; " +
                "UPDATE Reaction SET User_Id=null WHERE User_Id=@Id;" +
                "DELETE FROM Reaction WHERE Content_Id=(SELECT Id FROM Video WHERE User_Id=@id); " +
                "DELETE FROM Image_Post WHERE User_Id = @id; " +
                "DELETE FROM Video WHERE User_Id = @id; ";

            DeleteUserAndGetFilePath.Parameters.AddWithValue("@id", $"{id}");

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                string jsonFilePaths = databaseHandler.Select(DeleteUserAndGetFilePath);

                Logger.LogInformation(jsonFilePaths);

                var filePaths = JsonConvert.DeserializeObject<FilePathData[]>(jsonFilePaths);

                foreach (var paths in filePaths)
                {
                    System.IO.File.Delete(paths.File_Location);
                }

                Logger.Log(LogLevel.Information, "USER DELETE");
            }

            JWTtokenBlacklistItem jwttokenBlacklistItem = new JWTtokenBlacklistItem();

            if (token != null && jwttokenBlacklistItem != null)
            {
                Logger.LogInformation($"TOKEN = {token}");
                jwttokenBlacklistItem.Token = token;
                jwttokenBlacklistItem.ExpireTime = DateTime.Now.AddHours(8);


                JWTTokenHandler.BlackList!.Add(jwttokenBlacklistItem);
                JWTTokenHandler.WriteBlackList();
            }

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
        public Task<string> ChangePassword([FromBody] ChangePasswordObject newPassword)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id");

            string[] EncryptedPassword = EncryptionHandler.HashAndSaltData(newPassword.newPassword);

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

            string Userid = User.FindFirstValue("Id");


            using DatabaseHandler databaseHandler = new DatabaseHandler();

            using MySqlCommand CheckUsernameExist = new MySqlCommand();

            CheckUsernameExist.CommandText = "SELECT COUNT(*) FROM Username_Counter WHERE Username=@Username;";
            CheckUsernameExist.Parameters.AddWithValue("@Username", $"{newUsername}");

            CheckUsernameExist.Dispose();

            int Exist = databaseHandler.GetNumber(CheckUsernameExist);
            int newNumber = 0;
            using MySqlCommand GetUserNameAmount = new MySqlCommand();

            if (Exist == 0)
            {
                GetUserNameAmount.CommandText = "INSERT INTO Username_Counter (Username, Amount, Created_At, Updated_At) VALUES (@Username, @Amount, @Created_At, @Update_At);";

                GetUserNameAmount.Parameters.AddWithValue("@Username", $"{newUsername}");
                GetUserNameAmount.Parameters.AddWithValue("@Amount", 0);
                GetUserNameAmount.Parameters.AddWithValue("@Created_At", DateTime.Now);
                GetUserNameAmount.Parameters.AddWithValue("@Deleted_At", DateTime.Now);
                GetUserNameAmount.Parameters.AddWithValue("@Update_At", DateTime.Now);

                databaseHandler.EditDatabase(GetUserNameAmount);
            }
            else
            {

                GetUserNameAmount.CommandText = "SELECT Amount FROM Username_Counter WHERE Username=@Username;";

                GetUserNameAmount.Parameters.AddWithValue("@Username", $"{newUsername}");

                newNumber = databaseHandler.GetNumber(GetUserNameAmount);
            }
            GetUserNameAmount.Dispose();


            using MySqlCommand UpdateUsername = new MySqlCommand();
            UpdateUsername.CommandText = 
                "UPDATE Users_Public SET Username=@Username WHERE Id=@Id" +
                "UPDATE Username_Counter SET Amount=@Amount, Updated_At=@UpdatedAt WHERE Username=@Username;";

            UpdateUsername.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
            UpdateUsername.Parameters.AddWithValue("@Amount", newNumber + 1);
            UpdateUsername.Parameters.AddWithValue("@Username", $"{newUsername}#{(newNumber + 1).ToString("D4")}");
            UpdateUsername.Parameters.AddWithValue("@Id", Userid);

            databaseHandler.EditDatabase(UpdateUsername);
            UpdateUsername.Dispose();

            return Task.FromResult($"Username Updated");
        }
    }
}
