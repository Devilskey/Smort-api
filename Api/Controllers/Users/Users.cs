using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Tiktok_api.Auth;
using Tiktok_api.Services;
using Smort_api.Object;
using Smort_api.Object.Security;
using Smort_api.Object.User;
using System.Security.Claims;
using Tiktok_api.SignalRHubs;

namespace Tiktok_api.Controllers.Users
{
    /// <summary>
    /// Controller for user-related API endpoints.
    /// Handles user profiles, reports, and public user data retrieval.
    /// </summary>
    [ApiController]
    public partial class Users : ControllerBase
    {
        /// <summary>Logger instance for recording application events.</summary>
        private readonly ILogger Logger;
        
        /// <summary>Notification hub for sending real-time notifications.</summary>
        private readonly NotificationHubHandler _notificationHub;
        
        /// <summary>Mail handler for sending email notifications.</summary>
        private readonly MailHandler _mail;
        
        /// <summary>User service for business logic operations.</summary>
        private readonly IUserService _userService;

        /// <summary>Firebase authentication service for verifying tokens and generating local JWTs.</summary>

        /// <summary>Constructor - initializes the controller with dependencies.</summary>
        public Users(ILogger<Users> logger, NotificationHubHandler notificationHub, MailHandler mail, IUserService userService)
        {
            Logger = logger;
            _notificationHub = notificationHub;
            _mail = mail;
            _userService = userService;
        }

        /// <summary>
        /// Reports a user's account for violations.
        /// Requires authentication. Prevents duplicate reports from the same user.
        /// </summary>
        /// <param name="UserReported">User report object containing reported user ID and reason</param>
        /// <returns>Status message indicating if report was successful</returns>
        [Authorize]
        [Route("users/ReportUser")]
        [HttpPost]
        public async Task<string> ReportUser(ReportUser UserReported)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            int id = int.Parse(User.FindFirstValue("Id") ?? "0");

            if (string.IsNullOrEmpty(UserReported.Reason))
                return "User Reported";

            return await _userService.ReportUserAsync(id, UserReported.Id, UserReported.Reason);
        }

        /// <summary>
        /// Retrieves simplified public user data (profile picture and username).
        /// No authentication required for public data.
        /// </summary>
        /// <param name="userData">User data object containing user ID to retrieve</param>
        /// <returns>JSON-serialized user data or not found message</returns>
        [Route("users/GetUserDataSimpel")]
        [HttpPost]
        public async Task<string> GetUserDataSimpel(UserData userData)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            if (userData.Id == 0)
                return "Not valid value";

            var data = await _userService.GetUserDataSimpleAsync(userData.Id);
            return data == null ? "Not found" : JsonConvert.SerializeObject(data);
        }

        /// <summary>
        /// Retrieves the current authenticated user's profile data.
        /// Requires valid JWT authentication token.
        /// </summary>
        /// <returns>User profile object with ID, picture, and username</returns>
        [Authorize]
        [Route("users/GetMyProfile")]
        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return BadRequest("Token Black listed");

            var userIdClaim = User.FindFirstValue("app_user_id");

            if (!int.TryParse(userIdClaim, out var id))
                return Unauthorized("Invalid user identity");

            var userdata = await _userService.GetMyProfileAsync(id);
            Console.WriteLine(id);

            if (userdata == null)
                return BadRequest("Data not found");

            return Ok(userdata);
        }
        
        [Authorize]
        [Route("users/ConfigureUserData")]
        [HttpPost]
        public async Task<string> ConfigureUserData(CreateAccount createAccount)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";
            
            var userIdClaim = User.FindFirstValue("app_user_id");

            if (!int.TryParse(userIdClaim, out var id))
                return $"Invalid user  {userIdClaim} ";

            var data = await _userService.ConfigureUserData(id, createAccount);

            return data;
        }

        /// <summary>
        /// Retrieves user profile data for a specific user by ID.
        /// Publicly accessible endpoint for viewing user profiles.
        /// </summary>
        /// <param name="id">User ID to retrieve profile for</param>
        /// <returns>JSON-serialized user profile data</returns>
        [Route("users/GetUserDataProfile")]
        [HttpGet]
        public async Task<string> GetUserDataProfile(int id)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return "token is blacklisted";

            var data = await _userService.GetUserDataProfileAsync(id);
            return JsonConvert.SerializeObject(data);
        }
    }
}
