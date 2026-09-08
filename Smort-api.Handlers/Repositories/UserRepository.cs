using Dapper;
using MySql.Data.MySqlClient;
using Smort_api.Object.User;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Smort_api.Object;
using Smort_api.Object.DTO;
using Smort_api.Object.Videos;
using Tiktok_api.Settings_Api;

namespace Smort_api.Handlers.Repositories
{
    /// <summary>
    /// Repository for user-related database operations.
    /// Uses Dapper ORM for efficient, parameterized SQL queries.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        /// <summary>Database connection instance for all queries.</summary>
        private readonly IDbConnection _db;
        private readonly ILogger<UserRepository> _logger;
        private readonly MailHandler _mail;

        /// <summary>Constructor - initializes the repository with a database connection.</summary>
        public UserRepository(MySqlConnection connection,  ILogger<UserRepository> logger, MailHandler mail)
        {
            _mail = mail;
            _logger = logger;
            _db = connection;
        }

        /// <summary>Checks if a user has already reported another user.</summary>
        public async Task<int> GetReportCountAsync(int reporterId, int reportedId)
        {
            const string sql = "SELECT COUNT(*) FROM Report_User WHERE User_Reporter_Id=@IdReporter AND User_Reported_Id=@IdReported;";
            return await _db.ExecuteScalarAsync<int>(sql, new { IdReporter = reporterId, IdReported = reportedId });
        }

        /// <summary>Records a user report in the database.</summary>
        public async Task ReportUserAsync(int reportedId, int reporterId, string reason)
        {
            const string sql = "INSERT INTO Report_User (User_Reported_Id, User_Reporter_Id, Reason, Reported_At) VALUES (@IdReported, @IdReporter, @Reason, @ReportedAt);";
            await _db.ExecuteAsync(sql, new { IdReported = reportedId, IdReporter = reporterId, Reason = reason, ReportedAt = System.DateTime.UtcNow });
        }

        /// <summary>Retrieves the current user's profile data (ID, picture, username).</summary>
        public async Task<MyProfileDto?> GetMyProfileAsync(int id)
        {
            const string sql = "SELECT Id, Profile_Picture AS Profile_Picture, Username, Is_Account_Configured FROM Users_Public WHERE Id=@Id;";
            return await _db.QueryFirstOrDefaultAsync<MyProfileDto>(sql, new { Id = id });
        }

        /// <summary>Retrieves simplified user data for public profile viewing.</summary>
        public async Task<MyUserDataSimpelDto?> GetUserDataSimpleAsync(int id)
        {
            const string sql = "SELECT Id, Profile_Picture, Username FROM Users_Public WHERE Id=@Id;";
            return await _db.QueryFirstOrDefaultAsync<MyUserDataSimpelDto>(sql, new { Id = id });
        }

        /// <summary>Retrieves user profile data for display purposes.</summary>
        public async Task<IEnumerable<UserProfileDto>> GetUserDataProfileAsync(int id)
        {
            const string sql = "SELECT Id, Profile_Picture, Username FROM Users_Public WHERE Id=@Id;";
            return await _db.QueryAsync<UserProfileDto>(sql, new { Id = id });
        }

        public async Task AllowUserAsync(int userId, bool allow)
        {
            const string sql = "UPDATE Users_Public SET AllowedUser=@Allow WHERE Id=@Id;";
            await _db.ExecuteAsync(sql, new { Allow = allow ? 1 : 0, Id = userId });
        }

        public async Task<IEnumerable<AllUserDto>> GetAllUsersAsync()
        {
            const string sql = "SELECT Id, Profile_Picture, Username, Created_At, AllowedUser FROM Users_Public;";
            return await _db.QueryAsync<AllUserDto>(sql);
        }

        public async Task<IEnumerable<FilePathData>> GetUserFilePathsAsync(string userId)
        {
            const string sql = @"
                SELECT File_location FROM File WHERE Id=(SELECT Profile_Picture FROM Users_Public WHERE Id=@id)
                UNION
                SELECT File_location FROM File WHERE Id=(SELECT File_Id FROM Image_Post WHERE User_Id=@id)
                UNION
                SELECT File_location FROM File WHERE Id=(SELECT File_Id FROM Video WHERE User_Id=@id)
                UNION
                SELECT File_location FROM File WHERE Id=(SELECT Thumbnail FROM Video WHERE User_Id=@id);";

            return await _db.QueryAsync<FilePathData>(sql, new { id = userId });
        }

        public async Task DeleteUserAsync(string userId)
        {
            const string sql = @"
                DELETE FROM Users_Public WHERE Person_Id = @id;
                DELETE FROM Users_Private WHERE Id = @id;
                DELETE FROM Following WHERE User_Id_Followed = @id;
                DELETE FROM Following WHERE User_Id_Follower = @id;
                DELETE FROM Report_User WHERE User_Reported_Id = @id;
                DELETE FROM Report_User WHERE User_Reporter_Id = @id;
                UPDATE Reaction SET User_Id=null WHERE User_Id=@id;
                DELETE FROM Reaction WHERE Content_Id=(SELECT Id FROM Video WHERE User_Id=@id);
                DELETE FROM Image_Post WHERE User_Id = @id;
                DELETE FROM Video WHERE User_Id = @id;";

            await _db.ExecuteAsync(sql, new { id = userId });
        }

        public async Task ChangePasswordAsync(string id, string password, string salt)
        {
            const string sql = "UPDATE Users_Private SET Password=@Password, Salt=@Salt WHERE Id=@Id";
            await _db.ExecuteAsync(sql, new { Password = password, Salt = salt, Id = id });
        }

        public async Task ChangeEmailAsync(string id, string email)
        {
            const string sql = "UPDATE Users_Private SET Email=@Email WHERE Id=@Id";
            await _db.ExecuteAsync(sql, new { Email = email, Id = id });
        }

        public async Task ChangeProfilePictureAsync(string id, byte[] profilePicture)
        {
            const string sql = "UPDATE Users_Public SET Profile_Picture=@ProfilePicture WHERE Id=@Id";
            await _db.ExecuteAsync(sql, new { ProfilePicture = profilePicture, Id = id });
        }

        public async Task<int> GetUsernameCountAsync(string username)
        {
            const string sql = "SELECT COUNT(*) FROM Username_Counter WHERE Username=@Username;";
            return await _db.ExecuteScalarAsync<int>(sql, new { Username = username });
        }

        public async Task<int> GetUsernameAmountAsync(string username)
        {
            const string sql = "SELECT Amount FROM Username_Counter WHERE Username=@Username;";
            return await _db.ExecuteScalarAsync<int>(sql, new { Username = username });
        }

        public async Task InsertUsernameCounterAsync(string username)
        {
            const string sql = "INSERT INTO Username_Counter (Username, Amount, Created_At, Updated_At) VALUES (@Username, @Amount, @Created_At, @Updated_At);";
            await _db.ExecuteAsync(sql, new { Username = username, Amount = 0, Created_At = DateTime.Now, Updated_At = DateTime.Now });
        }

        public async Task UpdateUsernameAsync(string userId, string username)
        {
            const string sql = "UPDATE Users_Public SET Username=@Username WHERE Id=@Id;";
            await _db.ExecuteAsync(sql, new { Username = username, Id = userId });
        }

        public async Task UpdateUsernameCounterAsync(string username, int amount, DateTime updatedAt)
        {
            const string sql = "UPDATE Username_Counter SET Amount=@Amount, Updated_At=@UpdatedAt WHERE Username=@Username;";
            await _db.ExecuteAsync(sql, new { Username = username, Amount = amount, UpdatedAt = updatedAt });
        }

        public async Task<int> GetFollowCountAsync(string followerId, int followedUserId)
        {
            const string sql = "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Follower=@UserFollower AND User_Id_Followed=@UserFollowed;";
            return await _db.ExecuteScalarAsync<int>(sql, new { UserFollower = followerId, UserFollowed = followedUserId });
        }

        public async Task FollowUserAsync(string followerId, int followedUserId, DateTime followedAt)
        {
            const string sql = "INSERT INTO Following (User_Id_Followed, User_Id_Follower, Followed_At) VALUES (@UserFollowed, @UserFollower, @FollowedAt);";
            await _db.ExecuteAsync(sql, new { UserFollower = followerId, UserFollowed = followedUserId, FollowedAt = followedAt });
        }

        public async Task UnfollowUserAsync(int followedUserId, string followerId)
        {
            const string sql = "DELETE FROM Following WHERE User_Id_Followed=@UserFollowed AND User_Id_Follower=@UserFollower;";
            await _db.ExecuteAsync(sql, new { UserFollowed = followedUserId, UserFollower = followerId });
        }

        public async Task<int> GetFollowersCountAsync(int userId)
        {
            const string sql = "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Followed=@UserFollowed;";
            return await _db.ExecuteScalarAsync<int>(sql, new { UserFollowed = userId });
        }

        public async Task<int> GetMyFollowersCountAsync(string userId)
        {
            const string sql = "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Followed=@UserFollowed;";
            return await _db.ExecuteScalarAsync<int>(sql, new { UserFollowed = userId });
        }

        public async Task<IEnumerable<MostFollowersDto>> GetMostFollowersAsync(int offset)
        {
            const string sql = @"
                SELECT Following.User_Id_Followed, COUNT(User_Id_Follower) as Amount, Users_Public.Profile_Picture, Username
                FROM Following INNER JOIN Users_Public On Users_Public.Id = Following.User_Id_Followed
                GROUP BY User_Id_Followed ORDER BY Amount DESC LIMIT @Offset;";

            return await _db.QueryAsync<MostFollowersDto>(sql, new { Offset = offset });
        }

        public async Task<IEnumerable<MostFollowersDto>> GetFollowingAsync(string userId, int offset)
        {
            const string sql = @"
                SELECT Following.User_Id_Followed, COUNT(User_Id_Follower) as Amount, Users_Public.Profile_Picture, Username
                FROM Following INNER JOIN Users_Public On Users_Public.Id = Following.User_Id_Followed
                WHERE User_Id_Follower = @id
                GROUP BY User_Id_Followed ORDER BY Amount DESC LIMIT @Offset;";

            return await _db.QueryAsync<MostFollowersDto>(sql, new { Offset = offset, id = userId });
        }

        public async Task<bool> IsFollowingAsync(string userId, int followedUserId)
        {
            const string sql = "SELECT COUNT(User_Id_Followed) FROM Following WHERE User_Id_Followed=@UserFollowed AND User_Id_Follower=@UserFollower;";
            int follow = await _db.ExecuteScalarAsync<int>(sql, new { UserFollowed = followedUserId, UserFollower = userId });
            return follow != 0;
        }

        public async Task<string> ConfigureUserData(int id, CreateAccount createAccount)
        {

            const string sqlDoesUserNameExists = "SELECT COUNT(Person_Id) FROM Users_Public WHERE Username=@Username;";

            var exist = await _db.QueryAsync<int>(sqlDoesUserNameExists, new { Username = createAccount.Username });

            if (!exist.Any())
            {
                return "Username Already exists";
            }

            //Creates File
            const string sqlCreateFile =
                @"INSERT INTO File_Image (File_Name, File_Location, file_type_Id, Created_At) VALUES (@Name, @Location, @file_type_Id, @Created);
                  SELECT LAST_INSERT_ID();";
            
            var imageGUID = Guid.NewGuid().ToString();
            
            var fileId = await _db.QueryAsync<int>(sqlCreateFile, 
                new { Name = $"{imageGUID}.webp", Location=$"./ProfilePictures/{imageGUID}", file_type_Id=4, Created=DateTime.Now});
            
            foreach (var sizes in ContentSizingObjects.ProfilePictures)
            {
                float percentageLesser = ((float)sizes.Width / (float)createAccount.size.Width);

                int newWidth = (int)(percentageLesser * createAccount.size.Width);
                int newHeight = (int)(percentageLesser * createAccount.size.Height);

                var resizedFilePost = ImageHandler.ChangeSizeOfImage(createAccount.ProfilePicture, newWidth, newHeight);
                ImageHandler.SaveProfilePictures(resizedFilePost, $"{imageGUID}_{sizes.Size}.webp");
            }

            const string sqlUserData =
                "SELECT Id, Username, Profile_Picture AS Profile_Picture, Is_Account_Configured FROM Users_Public WHERE Id=@Id;";
                
            var user = await _db.QueryFirstOrDefaultAsync<MyProfileDto>(
                sqlUserData,
                new { Id = id });
            
            // Creates the new user and adds the data to the database
            if (user.IsAccountConfigured)
            {
                return "Account is Configured";
            }
            
            const string sqlAddUser = """
                                      UPDATE Users_Public
                                      SET
                                          Username = @Username,
                                          Profile_Picture = @ProfilePicture,
                                          Updated_At = @UpdatedAt,
                                          Is_Account_Configured = 1
                                      WHERE Person_Id = @PersonId;
                                      """;

            await _db.ExecuteAsync(
                sqlAddUser,
                new
                {
                    PersonId=id,
                    Username = createAccount.Username,
                    ProfilePicture = fileId.FirstOrDefault().ToString(),
                    UpdatedAt = DateTime.Now
                });
             
             
            // Logs the data 
            _logger.Log(LogLevel.Information, $"Created User: {createAccount.Username}");

            try
            {
                _mail.SendMail(
                    createAccount.Email,
                    "Hello, Your account is not yet active please wait for the admin to aprove your account",
                    "Welcome To Smorthub");
            }
            catch (Exception ex)
            {
                _logger.LogError($"MailError {ex.Message}");
            }
            // Send email to admin


            const string sqlGetAdministrator = @"SELECT Email FROM Users_Private WHERE Role_Id=3;";

            var jsonAdminMail = await _db.QueryAsync<EmailEntry>(sqlGetAdministrator);

            if (!jsonAdminMail.Any()) {
                _logger.LogError("NO ADMIN NO ADMIN ERROR ERROR SOMEONE FUCKED UP");
                return "User Created";
            }

            foreach (var entry in jsonAdminMail.ToList())
            {
                _mail.SendMail(
                    createAccount.Email,
                    $"Hello Admin, {createAccount.Username} is waiting to be aproved by U",
                    "New user waiting to be aproved");
            }

            return "User Created";
        }
    }
}
