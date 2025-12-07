using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object;
using Smort_api.Object.Security;
using Smort_api.Object.User;
using System.Security.Claims;
using Tiktok_api.SignalRHubs;

namespace Tiktok_api.Controllers.Users
{
    //When i made this only god and i knew how it worked.
    //Now only god knows

    [ApiController]
    public partial class Users : ControllerBase
    {
        private readonly ILogger Logger;
        private readonly NotificationHubHandler _notificationHub;
        private readonly MailHandler _mail;

        public Users(ILogger<Users> logger, NotificationHubHandler notificationHub, MailHandler mail)
        {
            Logger = logger;
            _notificationHub = notificationHub;
            _mail = mail;
        }

        /// <summary>
        /// Reports a users his account
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


            using MySqlCommand AlreadyReported = new MySqlCommand();
            AlreadyReported.CommandText = "SELECT COUNT(*) FROM Report_User WHERE User_Reporter_Id=@IdReporter AND User_Reported_Id=@IdReported;";
            AlreadyReported.Parameters.AddWithValue("@IdReporter", id);
            AlreadyReported.Parameters.AddWithValue("@IdReported", UserReported.Id);

            using MySqlCommand ReportUserCommand = new MySqlCommand();

            ReportUserCommand.CommandText = "INSERT INTO Report_User (User_Reported_Id, User_Reporter_Id, Reason, Reported_At) VALUES (@IdReported, @IdReporter, @Reason, @ReportedAt);";

            ReportUserCommand.Parameters.AddWithValue("@IdReported", UserReported.Id);
            ReportUserCommand.Parameters.AddWithValue("@IdReporter", id);

            ReportUserCommand.Parameters.AddWithValue("@Reason", UserReported.Reason);
            ReportUserCommand.Parameters.AddWithValue("@ReportedAt", DateTime.Now);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                if (databaseHandler.GetNumber(AlreadyReported) == 0)
                {
                    databaseHandler.EditDatabase(ReportUserCommand);
                    return Task.FromResult($"User Reported");
                }
            }

            return Task.FromResult($"User Already Reported by you");
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

            GetDataUser.CommandText = "SELECT Profile_Picture, Username FROM Users_Public WHERE Id=@Id;";
            GetDataUser.Parameters.AddWithValue("@Id", userData);

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
        public IActionResult GetMyProfile()
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return BadRequest("Token Black listed");

            string id = User.FindFirstValue("Id");

            using MySqlCommand GetDataUser = new MySqlCommand();

            GetDataUser.CommandText = "SELECT Id, Profile_Picture, Username FROM Users_Public WHERE Id=@id;";
            GetDataUser.Parameters.AddWithValue("Id", id);
            GetMyUserDataSimpel Userdata = new GetMyUserDataSimpel();

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                string jsonData = databaseHandler.Select(GetDataUser);
                GetMyUserDataSimpel[] UserDataArray = JsonConvert.DeserializeObject<GetMyUserDataSimpel[]>(jsonData);
                if (UserDataArray == null)
                    return BadRequest("Data not found");


                Userdata = UserDataArray[0];
            }


            return Ok(Userdata);
        }

        /// <summary>
        /// Returns UserData and videos that user has made
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        [Route("users/GetUserDataProfile")]
        [HttpGet]
        public Task<string> GetUserDataProfile(int id)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            using MySqlCommand GetDataUser = new MySqlCommand();

            GetDataUser.CommandText = "SELECT Profile_Picture, Username FROM Users_Public WHERE Id=@id;";
            GetDataUser.Parameters.AddWithValue("Id", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                return Task.FromResult(databaseHandler.Select(GetDataUser));
            }
        }
    }
}
