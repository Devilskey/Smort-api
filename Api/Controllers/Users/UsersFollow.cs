using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smort_api.Handlers;
using Smort_api.Object.DTO;
using Tiktok_api.Services;

namespace Tiktok_api.Controllers.Users
{
    [Authorize]
    public partial class Users : ControllerBase
    {
        [Route("users/FollowUser")]
        [HttpPost]
        public async Task<string> FollowUser(int id)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            string idUser = User.FindFirstValue("app_user_id");
            string username = User.FindFirstValue("Username");

            return await _userService.FollowUserAsync(idUser, id, username ?? string.Empty, _notificationHub);
        }

        [Route("users/UnFollowUser")]
        [HttpDelete]
        public async Task<string> UnFollowUser(int creatorId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            string id = User.FindFirstValue("app_user_id");
            return await _userService.UnfollowUserAsync(id, creatorId);
        }

        [Route("users/FollowersAmount")]
        [HttpPost]
        public async Task<ActionResult<int>> FollowersAmount(int id)
        {
            if (id == 0)
                return BadRequest();

            var count = await _userService.FollowersAmountAsync(id);
            return Ok(count);
        }

        [Route("Following/MostFolowers")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MostFollowersDto>>> MostFollowers(int Offset = 5)
        {
            var list = await _userService.MostFollowersAsync(Offset);
            return Ok(list);
        }

        [Route("Following/Following")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MostFollowersDto>>> Following(int Offset = 5)
        {
            string idUser = User.FindFirstValue("app_user_id");

            if (string.IsNullOrEmpty(idUser))
                return BadRequest();

            var list = await _userService.FollowingAsync(idUser, Offset);
            return Ok(list);
        }

        [Route("users/AlreadyFollowing")]
        [HttpPost]
        public async Task<ActionResult<bool>> AlreadyFollowing(int id)
        {
            string idUser = User.FindFirstValue("app_user_id");

            if (string.IsNullOrEmpty(idUser))
                return BadRequest();

            if (id == 0)
                return BadRequest();

            return await _userService.AlreadyFollowingAsync(idUser, id);
        }

        [Route("users/MyFollowersAmount")]
        [HttpGet]
        public async Task<ActionResult<int>> MyFollowersAmount()
        {
            string id = User.FindFirstValue("app_user_id");

            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Forbid();

            var count = await _userService.MyFollowersAmountAsync(id);
            return Ok(count);
        }
    }
}
