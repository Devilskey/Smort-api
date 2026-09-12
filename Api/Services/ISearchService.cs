using Smort_api.Object.DTO;

namespace Tiktok_api.Services;

public interface ISearchService
{ 
    Task<SearchAllDto> SearchAllAsync(string query, int userId); 
} 