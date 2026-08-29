using System.Threading.Tasks;

namespace Tiktok_api.Services
{
    public interface IAnalyticsService
    {
        Task AddViewAsync(string pageName);
        Task<int> AddPageAsync(string pageName);
    }
}
