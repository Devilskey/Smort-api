using Smort_api.Object.DTO;

namespace Smort_api.Handlers.Repositories;

public interface ISearchRepository
{
    Task<SearchAllDto> SearchAllAsync(string query, int userId);
}