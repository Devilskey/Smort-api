using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Smort_api.Handlers;
using Smort_api.Object.User;
using System.Security.Claims;

namespace Tiktok_api.Controllers.Users
{

    public partial class Users : ControllerBase
    {
        [Authorize]
        [HttpPost("Admin/users/PlatformAccess")]
        public ActionResult AllowUser(UserAllow user)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return BadRequest();

            string RoleId = User.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            Logger.LogInformation(RoleId);

            if (RoleId != "3")
            {
                return BadRequest();
            }

            using MySqlCommand DeleteUserAndGetFilePath = new MySqlCommand();

            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.CommandText = @"
                UPDATE Users_Public SET AllowedUser=@Allow WHERE Id=@Id;
            ";
            sqlCommand.Parameters.AddWithValue("@Allow", user.Allow ? 1 : 0);
            sqlCommand.Parameters.AddWithValue("@Id", user.Id);

            Logger.LogInformation(user.Id.ToString());
            Logger.LogInformation(user.Allow.ToString());


            using (DatabaseHandler database = new DatabaseHandler())
            {
                database.EditDatabase(sqlCommand);
                return Ok();
            }
        }

        [Authorize]
        [HttpGet("Admin/users/All")]
        public Task<string> GetAllUsers()
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string RoleId = User.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            Logger.LogInformation(RoleId);

            if(RoleId != "3")
            {
                return Task.FromResult("NO ACCESS");
            }

            using MySqlCommand DeleteUserAndGetFilePath = new MySqlCommand();

            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.CommandText = @"
                SELECT Id, Profile_Picture, Username, Created_At, AllowedUser FROM Users_Public;
            ";

            using (DatabaseHandler database = new DatabaseHandler())
            {
               return Task.FromResult(database.Select(sqlCommand));
            }
        }
    }
}
