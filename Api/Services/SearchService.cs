using System.Web.Http;
using Smort_api.Handlers.Repositories;
using Smort_api.Object.DTO;

namespace Tiktok_api.Services;

public class SearchService
    (ISearchRepository _searchRepository) : ISearchService
{
    public async Task<SearchAllDto> SearchAllAsync(string query, int userId)
    {
        var data = await _searchRepository.SearchAllAsync(query, userId);
        return data;
    }
}