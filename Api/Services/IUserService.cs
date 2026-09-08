using Smort_api.Object.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using Smort_api.Object;
using Smort_api.Object.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tiktok_api.SignalRHubs;

namespace Tiktok_api.Services
{
    /// <summary>
    /// Interface for user service operations.
    /// Defines contracts for user-related business logic.
    /// </summary>
    public interface IUserService
    {
        /// <summary>Reports a user with a specified reason.</summary>
        Task<string> ReportUserAsync(int reporterId, int reportedId, string reason);
        
        /// <summary>Gets the current user's profile data.</summary>
        Task<MyProfileDto?> GetMyProfileAsync(int id);
        
        /// <summary>Gets simplified user data for public viewing.</summary>
        Task<MyUserDataSimpelDto?> GetUserDataSimpleAsync(int id);
        
        /// <summary>Gets user profile data for display purposes.</summary>
        Task<IEnumerable<UserProfileDto>> GetUserDataProfileAsync(int id);
        Task<string> ConfigureUserData(int id, CreateAccount createAccount);
        Task AllowUserAsync(int userId, bool allow);
        Task<IEnumerable<AllUserDto>> GetAllUsersAsync();
        Task<string> DeleteUserAsync(string userId, string token, ILogger logger);
        Task<string> ChangePasswordAsync(string id, string newPassword);
        Task<string> ChangeEmailAsync(string id, string newEmail);
        Task<string> ChangeProfilePictureAsync(string id, byte[] newProfilePicture);
        Task<string> ChangeUsernameAsync(string userId, string newUsername);
        Task<string> FollowUserAsync(string currentUserId, int targetUserId, string username, NotificationHubHandler notificationHub);
        Task<string> UnfollowUserAsync(string currentUserId, int targetUserId);
        Task<int> FollowersAmountAsync(int userId);
        Task<IEnumerable<MostFollowersDto>> MostFollowersAsync(int offset);
        Task<IEnumerable<MostFollowersDto>> FollowingAsync(string userId, int offset);
        Task<ActionResult<bool>> AlreadyFollowingAsync(string userId, int targetUserId);
        Task<int> MyFollowersAmountAsync(string userId);
    }
}
