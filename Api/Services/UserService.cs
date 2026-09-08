using Smort_api.Handlers.Repositories;
using Smort_api.Object.User;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Smort_api.Object;
using Smort_api.Handlers;
using Microsoft.Extensions.Logging;
using Smort_api.Object.DTO;
using Smort_api.Object.Security;
using Microsoft.AspNetCore.Mvc;
using Tiktok_api.SignalRHubs;

namespace Tiktok_api.Services
{
    /// <summary>
    /// Service layer for user-related business logic.
    /// Orchestrates user operations and delegates data access to UserRepository.
    /// </summary>
    public class UserService : IUserService
    {
        /// <summary>Repository for user data access.</summary>
        private readonly IUserRepository _userRepository;

        /// <summary>Constructor - initializes the service with a user repository.</summary>
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>Reports a user after checking for existing reports.</summary>
        public async Task<string> ReportUserAsync(int reporterId, int reportedId, string reason)
        {
            var exists = await _userRepository.GetReportCountAsync(reporterId, reportedId);
            if (exists == 0)
            {
                await _userRepository.ReportUserAsync(reportedId, reporterId, reason);
                return "User Reported";
            }
            return "User Already Reported by you";
        }

        /// <summary>Retrieves current user's profile data.</summary>
        public async Task<MyProfileDto?> GetMyProfileAsync(int id)
        {
            return await _userRepository.GetMyProfileAsync(id);
        }

        /// <summary>Retrieves simplified user data for public viewing.</summary>
        public async Task<MyUserDataSimpelDto?> GetUserDataSimpleAsync(int id)
        {
            return await _userRepository.GetUserDataSimpleAsync(id);
        }

        /// <summary>Retrieves user profile data for display.</summary>
        public async Task<IEnumerable<UserProfileDto>> GetUserDataProfileAsync(int id)
        {
            return await _userRepository.GetUserDataProfileAsync(id);
        }

        public async Task<string> ConfigureUserData(int id, CreateAccount createAccount)
        {
            return await _userRepository.ConfigureUserData(id, createAccount);
        }

        public async Task AllowUserAsync(int userId, bool allow)
        {
            await _userRepository.AllowUserAsync(userId, allow);
        }

        public async Task<IEnumerable<AllUserDto>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<string> DeleteUserAsync(string userId, string token, ILogger logger)
        {
            var filePaths = await _userRepository.GetUserFilePathsAsync(userId);
            foreach (var path in filePaths.Where(p => !string.IsNullOrEmpty(p.File_Location) && File.Exists(p.File_Location)))
            {
                File.Delete(path.File_Location);
            }

            await _userRepository.DeleteUserAsync(userId);

            if (!string.IsNullOrWhiteSpace(token))
            {
                JWTTokenHandler.BlackList ??= new List<JWTtokenBlacklistItem>();

                JWTTokenHandler.BlackList.Add(new JWTtokenBlacklistItem
                {
                    Token = token,
                    ExpireTime = DateTime.Now.AddHours(8)
                });

                JWTTokenHandler.WriteBlackList();
                logger.LogInformation("USER DELETE");
            }

            return "User Removed";
        }

        public async Task<string> ChangePasswordAsync(string id, string newPassword)
        {
            var encryptedPassword = EncryptionHandler.HashAndSaltData(newPassword);
            await _userRepository.ChangePasswordAsync(id, encryptedPassword[1], encryptedPassword[0]);
            return "Password Changed";
        }

        public async Task<string> ChangeEmailAsync(string id, string newEmail)
        {
            await _userRepository.ChangeEmailAsync(id, newEmail);
            return "Email Updated";
        }

        public async Task<string> ChangeProfilePictureAsync(string id, byte[] newProfilePicture)
        {
            await _userRepository.ChangeProfilePictureAsync(id, newProfilePicture);
            return "Profile_Picture Updated";
        }

        public async Task<string> ChangeUsernameAsync(string userId, string newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername))
                throw new ArgumentException("Username is required.", nameof(newUsername));

            int exist = await _userRepository.GetUsernameCountAsync(newUsername);
            int newNumber = 0;

            if (exist == 0)
            {
                await _userRepository.InsertUsernameCounterAsync(newUsername);
            }
            else
            {
                newNumber = await _userRepository.GetUsernameAmountAsync(newUsername);
            }

            string updatedUsername = $"{newUsername}#{(newNumber + 1).ToString("D4")}";
            await _userRepository.UpdateUsernameAsync(userId, updatedUsername);
            await _userRepository.UpdateUsernameCounterAsync(newUsername, newNumber + 1, DateTime.Now);

            return "Username Updated";
        }

        public async Task<string> FollowUserAsync(string currentUserId, int targetUserId, string username, NotificationHubHandler notificationHub)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new ArgumentException("User id is required.", nameof(currentUserId));

            if (targetUserId == 0)
                return "Failed to follow user";

            if (int.TryParse(currentUserId, out var currentUserIdInt) && currentUserIdInt == targetUserId)
                return "you cannnot follow yourself";

            if (await _userRepository.GetFollowCountAsync(currentUserId, targetUserId) == 0)
            {
                await _userRepository.FollowUserAsync(currentUserId, targetUserId, DateTime.Now);
                return "Now following user";
            }

            notificationHub.SendNotificationFollowToUser(targetUserId.ToString(), $"{username} started following you");
            return "Not able to follow this user";
        }

        public async Task<string> UnfollowUserAsync(string currentUserId, int targetUserId)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new ArgumentException("User id is required.", nameof(currentUserId));

            if (targetUserId == 0)
                return "Failed to follow user";

            await _userRepository.UnfollowUserAsync(targetUserId, currentUserId);
            return "user Unfollowed";
        }

        public async Task<int> FollowersAmountAsync(int userId)
        {
            return await _userRepository.GetFollowersCountAsync(userId);
        }

        public async Task<IEnumerable<MostFollowersDto>> MostFollowersAsync(int offset)
        {
            return await _userRepository.GetMostFollowersAsync(offset);
        }

        public async Task<IEnumerable<MostFollowersDto>> FollowingAsync(string userId, int offset)
        {
            return await _userRepository.GetFollowingAsync(userId, offset);
        }

        public async Task<ActionResult<bool>> AlreadyFollowingAsync(string userId, int targetUserId)
        {
            if (string.IsNullOrWhiteSpace(userId) || targetUserId == 0)
                return new BadRequestObjectResult(false);

            var result = await _userRepository.IsFollowingAsync(userId, targetUserId);
            return new OkObjectResult(result);
        }

        public async Task<int> MyFollowersAmountAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User id is required.", nameof(userId));

            return await _userRepository.GetMyFollowersCountAsync(userId);
        }
    }
}
