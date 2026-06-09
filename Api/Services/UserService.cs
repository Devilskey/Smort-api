using Smort_api.Handlers.Repositories;
using Smort_api.Object.User;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public async Task<GetMyUserDataSimpel?> GetMyProfileAsync(int id)
        {
            return await _userRepository.GetMyProfileAsync(id);
        }

        /// <summary>Retrieves simplified user data for public viewing.</summary>
        public async Task<GetMyUserDataSimpel?> GetUserDataSimpleAsync(int id)
        {
            return await _userRepository.GetUserDataSimpleAsync(id);
        }

        /// <summary>Retrieves user profile data for display.</summary>
        public async Task<IEnumerable<GetMyUserDataSimpel>> GetUserDataProfileAsync(int id)
        {
            return await _userRepository.GetUserDataProfileAsync(id);
        }
    }
}
