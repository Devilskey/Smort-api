using Smort_api.Object.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using Smort_api.Object;

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
    }
}
