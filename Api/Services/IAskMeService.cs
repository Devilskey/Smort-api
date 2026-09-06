using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tiktok_api.Services
{
    public interface IAskMeService
    {
        Task CreateQuestionAsync(string userId, string content);
        Task CreateAnswerAsync(string userId, int askId, string content);
        Task<IEnumerable<object>> GetAnswersAsync(int askId);
    }
}
