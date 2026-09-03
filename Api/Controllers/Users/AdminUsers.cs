using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Smort_api.Handlers;
using Smort_api.Object.User;
using System.Security.Claims;
using Dapper;

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
            
            if (RoleId != "3")
            {
                return BadRequest();
            }
            
            _db.ExecuteAsync("Users_Public SET AllowedUser=@Allow WHERE Id=@Id;", new {Allow= user.Allow ? 1 : 0, Id=user.Id });
            return Ok();
        }

        [Authorize]
        [HttpGet("Admin/users/All")]
        public async Task<object> GetAllUsers()
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            string roleId = User.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            Logger.LogInformation(roleId);

            if(roleId != "3")
            {
                return "token is blacklisted";
            }

            var sqlGetAllUsers = @"SELECT Id, Profile_Picture, Username, Created_At, AllowedUser FROM Users_Public;";
            
            return await _db.QueryAsync<object>(sqlGetAllUsers);
        }
    }
}
