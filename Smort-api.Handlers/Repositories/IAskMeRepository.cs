using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    public interface IAskMeRepository
    {
        Task CreateQuestionAsync(string userId, string content);
        Task CreateAnswerAsync(string userId, int askId, string content);
        Task<IEnumerable<object>> GetAnswersByQuestionIdAsync(int askId);
    }
}
