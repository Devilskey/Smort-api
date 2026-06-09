using Smort_api.Object.User;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task<GetMyUserDataSimpel?> GetMyProfileAsync(int id);
        
        /// <summary>Gets simplified user data for public viewing.</summary>
        Task<GetMyUserDataSimpel?> GetUserDataSimpleAsync(int id);
        
        /// <summary>Gets user profile data for display purposes.</summary>
        Task<IEnumerable<GetMyUserDataSimpel>> GetUserDataProfileAsync(int id);
    }
}
