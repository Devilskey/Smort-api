using Smort_api.Handlers.Repositories;
using System.Threading.Tasks;

namespace Tiktok_api.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _repo;

        public AnalyticsService(IAnalyticsRepository repo)
        {
            _repo = repo;
        }

        public async Task AddViewAsync(string pageName)
        {
            var exists = await _repo.PageExistsAsync(pageName);
            int? pageId = await _repo.GetPageIdAsync(pageName);
            if (!exists || pageId == null)
            {
                pageId = await _repo.CreatePageAsync(pageName);
            }

            await _repo.IncrementMonthlyViewAsync(pageId.Value);
        }

        public async Task<int> AddPageAsync(string pageName)
        {
            return await _repo.CreatePageAsync(pageName);
        }
    }
}
