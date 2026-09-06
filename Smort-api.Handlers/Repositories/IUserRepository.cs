using Smort_api.Object.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using Smort_api.Object;
using Smort_api.Object.Videos;

namespace Smort_api.Handlers.Repositories
{
    public interface IUserRepository
    {
        Task<int> GetReportCountAsync(int reporterId, int reportedId);
        Task ReportUserAsync(int reportedId, int reporterId, string reason);
        Task<GetMyUserDataSimpel?> GetMyProfileAsync(int id);
        Task<GetMyUserDataSimpel?> GetUserDataSimpleAsync(int id);
        Task<IEnumerable<GetMyUserDataSimpel>> GetUserDataProfileAsync(int id);
        Task<string> ConfigureUserData(int id, CreateAccount createAccount);
        Task AllowUserAsync(int userId, bool allow);
        Task<IEnumerable<object>> GetAllUsersAsync();
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
    }
}
