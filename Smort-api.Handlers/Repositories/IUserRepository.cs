using Smort_api.Object.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using Smort_api.Object;
using Smort_api.Object.DTO;
using Smort_api.Object.Videos;

namespace Smort_api.Handlers.Repositories
{
    public interface IUserRepository
    {
        Task<int> GetReportCountAsync(int reporterId, int reportedId);
        Task ReportUserAsync(int reportedId, int reporterId, string reason);
        Task<MyProfileDto?> GetMyProfileAsync(int id);
        Task<MyUserDataSimpelDto?> GetUserDataSimpleAsync(int id);
        Task<IEnumerable<UserProfileDto>> GetUserDataProfileAsync(int id);
        Task<string> ConfigureUserData(int id, CreateAccount createAccount);
        Task AllowUserAsync(int userId, bool allow);
        Task<IEnumerable<AllUserDto>> GetAllUsersAsync();
        Task DeleteUserAsync(string userId);
        Task ChangePasswordAsync(string id, string password, string salt);
        Task ChangeEmailAsync(string id, string email);
        Task ChangeProfilePictureAsync(string id, byte[] profilePicture);
        Task<int> GetUsernameCountAsync(string username);
        Task<int> GetUsernameAmountAsync(string username);
        Task InsertUsernameCounterAsync(string username);
        Task UpdateUsernameAsync(string userId, string username);
        Task<IEnumerable<FilePathData>> GetUserFilePathsAsync(string userId);
        Task UpdateUsernameCounterAsync(string username, int amount, DateTime updatedAt);
        Task<int> GetFollowCountAsync(string followerId, int followedUserId);
        Task FollowUserAsync(string followerId, int followedUserId, DateTime followedAt);
        Task UnfollowUserAsync(int followedUserId, string followerId);
        Task<int> GetFollowersCountAsync(int userId);
        Task<int> GetMyFollowersCountAsync(string userId);
        Task<IEnumerable<MostFollowersDto>> GetMostFollowersAsync(int offset);
        Task<IEnumerable<MostFollowersDto>> GetFollowingAsync(string userId, int offset);
        Task<bool> IsFollowingAsync(string userId, int followedUserId);
    }
}
