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
using Dapper;

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
        public async Task<string> Delete()
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            string id = User.FindFirstValue("app_user_id");

            using MySqlCommand DeleteUserAndGetFilePath = new MySqlCommand();

            var sqlQuery =
                @"SELECT File_location FROM File WHERE Id=(SELECT Profile_Picture FROM Users_Public WHERE Id=@id) UNION 
                SELECT File_location FROM File WHERE Id=(SELECT File_Id FROM Image_Post WHERE User_Id=@id) UNION 
                SELECT File_location FROM File WHERE Id=(SELECT File_Id FROM Video WHERE User_Id=@id) UNION 
                SELECT File_location FROM File WHERE Id=(SELECT Thumbnail FROM Video WHERE User_Id=@id); 
                DELETE FROM Users_Public WHERE Person_Id = @id; DELETE FROM Users_Private WHERE Id = @id;
                DELETE FROM Following WHERE User_Id_Followed = @id; DELETE FROM Following WHERE User_Id_Follower = @id; 
                DELETE FROM Report_User WHERE User_Reported_Id = @id; DELETE FROM Report_User WHERE User_Reporter_Id = @id; 
                UPDATE Reaction SET User_Id=null WHERE User_Id=@Id;
                DELETE FROM Reaction WHERE Content_Id=(SELECT Id FROM Video WHERE User_Id=@id);
                DELETE FROM Image_Post WHERE User_Id = @id;
                DELETE FROM Video WHERE User_Id = @id; ";
            
            var filePaths = await _db.QueryAsync<FilePathData>(sqlQuery, new { id = id });
            
            foreach (var paths in filePaths)
            {
                System.IO.File.Delete(paths.File_Location);
            }
            
            Logger.Log(LogLevel.Information, "USER DELETE");
            
            JWTtokenBlacklistItem jwttokenBlacklistItem = new JWTtokenBlacklistItem();

            if (token != null && jwttokenBlacklistItem != null)
            {
                Logger.LogInformation($"TOKEN = {token}");
                jwttokenBlacklistItem.Token = token;
                jwttokenBlacklistItem.ExpireTime = DateTime.Now.AddHours(8);

                JWTTokenHandler.BlackList!.Add(jwttokenBlacklistItem);
                JWTTokenHandler.WriteBlackList();
            }

            return "User Removed";
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

            string id = User.FindFirstValue("app_user_id");

            string[] EncryptedPassword = EncryptionHandler.HashAndSaltData(newPassword.newPassword);
            
            var sql = "UPDATE Users_Private SET Password=@Password, Salt=@Salt WHERE Id=@Id";
            
            _db.ExecuteAsync(sql, new {Password=EncryptedPassword[1], Salt=EncryptedPassword[0], Id=id});
            
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

            string id = User.FindFirstValue("app_user_id");
                
            var sql = "UPDATE Users_Private SET Email=@Email WHERE Id=@Id";

            _db.ExecuteAsync(sql, new {Email = newEmail, Id = id});
            
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

            string id = User.FindFirstValue("app_user_id");
            
            var sql = "UPDATE Users_Public SET Profile_Picture=@ProfilePicture WHERE Id=@Id";

            _db.ExecuteAsync(sql, new { ProfilePicture = newProfilePicture, Id = id });

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
        public async Task<string> ChangeUsername(string newUsername)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";
            
            var sqlUsernameCounter = "SELECT COUNT(*) FROM Username_Counter WHERE Username=@Username;";

            var exist = await _db.QueryAsync<int>(sqlUsernameCounter, new { Username = newUsername });
            
            int newNumber = 0;

            if (exist.FirstOrDefault() != 0)
            {
                var sqlInsert = "INSERT INTO Username_Counter (Username, Amount, Created_At, Updated_At) VALUES (@Username, @Amount, @Created_At, @Update_At);";
                await _db.QueryAsync(sqlInsert, new { Username = newUsername, Amount = newNumber, Created_At = DateTime.Now, Updated_At = DateTime.Now });
            }
            else
            {
                var sqlSelect = "SELECT Amount FROM Username_Counter WHERE Username=@Username;";
                await _db.QueryAsync(sqlSelect, new  { Username = newUsername });
            }
            
            var sqlUpdateUsername = 
                @"UPDATE Users_Public SET Username=@Username WHERE Id=@Id;
                  UPDATE Username_Counter SET Amount=@Amount, Updated_At=@UpdatedAt WHERE Username=@Username;";

            _db.QueryAsync(sqlUpdateUsername, new { Username = newUsername, UpdatedAt=DateTime.Now, Amount = newNumber, CreatedAt = DateTime.Now });
            
            return $"Username Updated";
        }
    }
}
