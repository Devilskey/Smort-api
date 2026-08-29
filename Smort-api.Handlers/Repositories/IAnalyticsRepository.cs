using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    public interface IAnalyticsRepository
    {
        Task<bool> PageExistsAsync(string pageName);
        Task<int?> GetPageIdAsync(string pageName);
        Task IncrementMonthlyViewAsync(int pageId);
        Task<int> CreatePageAsync(string pageName);
    }
}
