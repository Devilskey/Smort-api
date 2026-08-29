using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    public interface IReactionsRepository
    {
        Task<int> GetReactionCountAsync(string userId, string contentId, string reaction, string type);
        Task AddReactionAsync(string userId, string contentId, string type, string reaction);
        Task RemoveReactionAsync(string userId, string contentId, string type, string reaction);
        Task<(int Id, string Username)?> GetContentOwnerAsync(string contentId);
    }
}
