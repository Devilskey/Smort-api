using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object;
using Smort_api.Object.Security;
using Smort_api.Object.User;
using System.Security.Claims;

namespace Tiktok_api.Controllers
{
    [ApiController]
    public class Users : ControllerBase
    {
        private readonly ILogger Logger;

        public Users(ILogger<Users> logger) {
            Logger = logger;
        }

        [Route("users/CreateAccount")]
        [HttpPost]
        public Task<ActionResult> CreateAccount(CreateAccount newUser)
        {
            // Checks if data is not empty
            if (string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.Password) || string.IsNullOrEmpty(newUser.Username))
                return Task.FromResult<ActionResult>(BadRequest("Not all the data is Filled in"));

            // Makes the using for the databasehandler that will be disposed when the task is done.
            using DatabaseHandler databaseHandler = new DatabaseHandler();

            // Checks if the email adress is already used in the database
            using MySqlCommand CheckIfExist = new MySqlCommand();
            CheckIfExist.CommandText = "SELECT Email FROM Users_Private WHERE Email = @Email;";
            CheckIfExist.Parameters.AddWithValue("@Email", newUser.Email);

            string data = databaseHandler.Select(CheckIfExist);

            CheckIfExist.Dispose();

            LoginObject[] Emails = JsonConvert.DeserializeObject<LoginObject[]>(data)!;

            // if the email adress already has been used return a bad request
            if (Emails != null && Emails.Length != 0)
                return Task.FromResult<ActionResult>(BadRequest("Already an account using this Email"));

            using MySqlCommand CheckUsernameExist = new MySqlCommand();

            CheckUsernameExist.CommandText = "SELECT COUNT(*) FROM Username_Counter WHERE Username=@Username;";
            CheckUsernameExist.Parameters.AddWithValue("@Username", $"{newUser.Username}");

            CheckUsernameExist.Dispose();

            int Exist = databaseHandler.GetNumber(CheckUsernameExist);
            int id = 1;

            if (Exist == 0)
            {
                //Insert New Username
                using MySqlCommand GetUserNameAmount = new MySqlCommand();

                GetUserNameAmount.CommandText = "INSERT INTO Username_Counter (Username, Amount, Created_At, Deleted_At, Updated_At) VALUES (@Username, @Amount, @Created_At, @Deleted_At, @Update_At);";

                GetUserNameAmount.Parameters.AddWithValue("@Username", $"{newUser.Username}");
                GetUserNameAmount.Parameters.AddWithValue("@Amount", 0);
                GetUserNameAmount.Parameters.AddWithValue("@Created_At", DateTime.Now);
                GetUserNameAmount.Parameters.AddWithValue("@Deleted_At", DateTime.Now);
                GetUserNameAmount.Parameters.AddWithValue("@Update_At", DateTime.Now);

                databaseHandler.EditDatabase(GetUserNameAmount);
                GetUserNameAmount.Dispose();
            }
            else
            {
                using MySqlCommand GetUserNameAmount = new MySqlCommand();

                GetUserNameAmount.CommandText = "SELECT Amount FROM Username_Counter WHERE Username=@Username;";

                GetUserNameAmount.Parameters.AddWithValue("@Username", $"{newUser.Username}");

                id = databaseHandler.GetNumber(GetUserNameAmount);
                GetUserNameAmount.Dispose();
            }

            // Creates the new user and adds the data to the database
            using MySqlCommand addUser = new MySqlCommand();

            string[] Results = EncryptionHandler.HashAndSaltData(newUser.Password);

            addUser.CommandText =
                @"INSERT INTO Users_Private (Role_Id, Email, Password, Salt)
                 VALUES (1, @Email, @Password, @Salt);
                 INSERT INTO Users_Public (Person_Id, Username, Profile_Picture, Created_At, Updated_At, Deleted_At) 
                 VALUES (LAST_INSERT_ID(), @Username, @ProfilePicture, @CreatedAt, @UpdatedAt, @DeletedAt);";

            addUser.Parameters.AddWithValue("@Email", newUser.Email);
            addUser.Parameters.AddWithValue("@Password", Results[1]);
            addUser.Parameters.AddWithValue("@Salt", Results[0]);
            addUser.Parameters.AddWithValue("@Username", $"{newUser.Username}#{(id + 1).ToString("D4")}");
            addUser.Parameters.AddWithValue("@ProfilePicture", newUser.ProfilePicture);
            addUser.Parameters.AddWithValue("@DeletedAt", DateTime.Now);
            addUser.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
            addUser.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            databaseHandler.EditDatabase(addUser);

            addUser.Dispose();

            using MySqlCommand UpdateUsernameAmount = new MySqlCommand();

            UpdateUsernameAmount.CommandText = "UPDATE Username_Counter SET Amount=@Amount, Updated_At=@UpdatedAt WHERE Username=@Username";

            UpdateUsernameAmount.Parameters.AddWithValue("@Username", $"{newUser.Username}");
            UpdateUsernameAmount.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

            UpdateUsernameAmount.Parameters.AddWithValue("@Amount", (id += 1));


            databaseHandler.EditDatabase(UpdateUsernameAmount);
            UpdateUsernameAmount.Dispose();

            // Logs the data 
            Logger.Log(LogLevel.Information, $"Created User: {newUser.Username}");

            return Task.FromResult<ActionResult>(Ok("User Created"));
        }

        [Route("users/Login")]
        [HttpPost]
        public Task<string> Login(LoginObject User)
        {
            // Checks if the data that has been received is not empty
            if (string.IsNullOrEmpty(User.Email) || string.IsNullOrEmpty(User.Password))
                return Task.FromResult("Data received Empty");

            // Checks if the email adress exists
            using MySqlCommand addUser = new MySqlCommand();
            addUser.CommandText =
                @"SELECT Password, Salt FROM Users_Private WHERE Email = @EmailGiven;";

            addUser.Parameters.AddWithValue("@EmailGiven", User.Email);

            string jsonData = "[{}]";

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                jsonData = databaseHandler.Select(addUser);
            }

            MySqlCommand getId = new MySqlCommand();
            getId.CommandText = "SELECT Id FROM Users_Private WHERE Email=@Email;";

            getId.Parameters.AddWithValue("@Email", User.Email);

            int id = -1;

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                id = databaseHandler.GetNumber(getId);
            }

            if (id == -1) return Task.FromResult($"User Id not found");

            PasswordObject[]? Passwords = JsonConvert.DeserializeObject<PasswordObject[]>(jsonData);


            if (Passwords == null || Passwords.Length == 0)
                return Task.FromResult("No accounts found");

            // Hashes the the password that has been givens
            string[] Results = EncryptionHandler.HashAndSaltData(User.Password);

            if (EncryptionHandler.VerifyData(Passwords[0], User.Password))
            {
                string token = JWTTokenHandler.GenerateToken(User, id.ToString());
                return Task.FromResult(token);
            }

            Logger.Log(LogLevel.Warning, $"Failed Login attempt at {DateTime.Now}");

            return Task.FromResult("Failed to login");
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        [Authorize]
        [Route("users/ReportUser")]
        [HttpPost]
        public Task<string> ReportUser(ReportUser UserReported)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id");

            if (string.IsNullOrEmpty(UserReported.Reason))
                return Task.FromResult($"User Reported");

            using MySqlCommand ReportUserCommand = new MySqlCommand();

            ReportUserCommand.CommandText = "INSERT INTO Report_User (User_Reported_Id, User_Reporter_Id, Reason, Reported_At) VALUES (@IdReported, @IdReporter, @Reason, @ReportedAt);";

            ReportUserCommand.Parameters.AddWithValue("@IdReported", UserReported.Id);
            ReportUserCommand.Parameters.AddWithValue("@IdReporter", id);

            ReportUserCommand.Parameters.AddWithValue("@Reason", UserReported.Reason);
            ReportUserCommand.Parameters.AddWithValue("@ReportedAt", DateTime.Now);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                databaseHandler.EditDatabase(ReportUserCommand);
            }

            return Task.FromResult($"User Reported");
        }

        [Authorize]
        [Route("users/FollowUser")]
        [HttpPost]
        public Task<string> FollowUser(UserData CreatorData)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id");

            if (CreatorData.Id == 0)
                return Task.FromResult($"Failed to follow user");

            using MySqlCommand ReportUserCommand = new MySqlCommand();

            ReportUserCommand.CommandText = "INSERT INTO Following (User_Id_Followed, User_Id_Follower, Followed_At) VALUES (@UserFollowed, @UserFollower, @FollowedAt);";

            ReportUserCommand.Parameters.AddWithValue("@UserFollowed", CreatorData.Id);
            ReportUserCommand.Parameters.AddWithValue("@UserFollower", id);

            ReportUserCommand.Parameters.AddWithValue("@FollowedAt", DateTime.Now);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                databaseHandler.EditDatabase(ReportUserCommand);
            }

            return Task.FromResult($"Now following user");
        }

        [Authorize]
        [Route("users/UnFollowUser")]
        [HttpDelete]
        public Task<string> UnFollowUser(UserData CreatorData)
        {
            return Task.FromResult($"Now following user");
        }

        /// <summary>
        /// Returns ProfilePicture and username
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        [Route("users/GetUserDataSimpel")]
        [HttpPost]
        public Task<string> GetUserDataSimpel(UserData userData)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            if (userData.Id == 0)
                return Task.FromResult($"Not valid value");

            using MySqlCommand GetDataUser = new MySqlCommand();

            GetDataUser.CommandText = "SELECT Profile_Picture, Username FROM Users_Public WHERE Id=@id;";
            GetDataUser.Parameters.AddWithValue("Id", userData);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                return Task.FromResult(databaseHandler.Select(GetDataUser));
            }
        }

        /// <summary>
        /// Returns ProfilePicture and username from the user 
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        [Authorize]
        [Route("users/GetMyProfile")]
        [HttpGet]
        public Task<string> GetMyProfile()
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id"); 

            using MySqlCommand GetDataUser = new MySqlCommand();

            GetDataUser.CommandText = "SELECT Profile_Picture, Username FROM Users_Public WHERE Id=@id;";
            GetDataUser.Parameters.AddWithValue("Id", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                return Task.FromResult(databaseHandler.Select(GetDataUser));
            }
        }

        /// <summary>
        /// Returns UserData and videos that user has made
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        [Route("users/GetUserDataProfile")]
        [HttpGet]
        public Task<string> GetUserDataProfile(UserData userData)
        {
            //TODO
            return Task.FromResult($"Needs To Be added when videos are done");

        }
    }
}
