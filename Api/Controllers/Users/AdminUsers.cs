using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smort_api.Handlers;
using Smort_api.Object.User;
using Smort_api.Object.DTO;
using System.Security.Claims;

namespace Tiktok_api.Controllers.Users
{
    public partial class Users : ControllerBase
    {
        [Authorize(Roles = "3")]
        [HttpPost("Admin/users/PlatformAccess")]
        public async Task<ActionResult> AllowUser(UserAllow user)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return BadRequest();

            await _userService.AllowUserAsync(user.Id, user.Allow);
            return Ok();
        }

        [Authorize(Roles = "3")]
        [HttpGet("Admin/users/All")]
        public async Task<ActionResult<IEnumerable<AllUserDto>>> GetAllUsers()
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Forbid();
            
            var data = await _userService.GetAllUsersAsync();
            return Ok(data);
        }
    }
}
