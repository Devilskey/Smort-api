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
        public async Task<GetMyUserDataSimpel?> GetMyProfileAsync(int id)
        {
            const string sql = "SELECT Id, Profile_Picture, Username, Is_Account_Configured FROM Users_Public WHERE Id=@Id;";
            return await _db.QueryFirstOrDefaultAsync<GetMyUserDataSimpel>(sql, new { Id = id });
        }

        /// <summary>Retrieves simplified user data for public profile viewing.</summary>
        public async Task<GetMyUserDataSimpel?> GetUserDataSimpleAsync(int id)
        {
            const string sql = "SELECT Id, Profile_Picture, Username FROM Users_Public WHERE Id=@Id;";
            return await _db.QueryFirstOrDefaultAsync<GetMyUserDataSimpel>(sql, new { Id = id });
        }

        /// <summary>Retrieves user profile data for display purposes.</summary>
        public async Task<IEnumerable<GetMyUserDataSimpel>> GetUserDataProfileAsync(int id)
        {
            const string sql = "SELECT Id, Profile_Picture, Username FROM Users_Public WHERE Id=@Id;";
            return await _db.QueryAsync<GetMyUserDataSimpel>(sql, new { Id = id });
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
                
            var user = await _db.QueryFirstOrDefaultAsync<GetMyUserDataSimpel>(
                sqlUserData,
                new { Id = id });
            
            // Creates the new user and adds the data to the database
            if (user.Is_Account_Configured)
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
