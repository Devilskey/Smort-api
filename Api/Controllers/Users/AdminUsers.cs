using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smort_api.Handlers;
using Smort_api.Object.User;
using System.Security.Claims;

namespace Tiktok_api.Controllers.Users
{
    public partial class Users : ControllerBase
    {
        [Authorize]
        [HttpPost("Admin/users/PlatformAccess")]
        public async Task<ActionResult> AllowUser(UserAllow user)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return BadRequest();

            string roleId = User.FindFirstValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

            if (roleId != "3")
                return BadRequest();

            await _userService.AllowUserAsync(user.Id, user.Allow);
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

            if (roleId != "3")
                return "token is blacklisted";

            return await _userService.GetAllUsersAsync();
        }
    }
}
