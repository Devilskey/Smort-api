using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smort_api.Handlers;
using Smort_api.Object.User;

namespace Tiktok_api.Controllers.Users
{
    [Authorize]
    public partial class Users : ControllerBase
    {
        [Route("users/DeleteUser")]
        [HttpDelete]
        public async Task<string> Delete()
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            string id = User.FindFirstValue("app_user_id");
            return await _userService.DeleteUserAsync(id, token, Logger);
        }

        [Route("users/ChangePassword")]
        [HttpPut]
        public async Task<string> ChangePassword([FromBody] ChangePasswordObject newPassword)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            string id = User.FindFirstValue("app_user_id");
            return await _userService.ChangePasswordAsync(id, newPassword.newPassword);
        }

        [Route("users/ChangeEmail")]
        [HttpPut]
        public async Task<string> ChangeEmail(string newEmail)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            string id = User.FindFirstValue("app_user_id");
            return await _userService.ChangeEmailAsync(id, newEmail);
        }

        [Route("users/ChangeProfilePicture")]
        [HttpPut]
        public async Task<string> ChangeProfilePicture(byte[] newProfilePicture)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            string id = User.FindFirstValue("app_user_id");
            return await _userService.ChangeProfilePictureAsync(id, newProfilePicture);
        }

        [Route("users/ChangeUsername")]
        [HttpPut]
        public async Task<string> ChangeUsername(string newUsername)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            string userId = User.FindFirstValue("app_user_id");
            return await _userService.ChangeUsernameAsync(userId, newUsername);
        }
    }
}
