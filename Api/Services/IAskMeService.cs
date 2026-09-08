using System.Collections.Generic;
using System.Threading.Tasks;
using Smort_api.Object.DTO;

namespace Tiktok_api.Services
{
    public interface IAskMeService
    {
        Task CreateQuestionAsync(string userId, string content);
        Task CreateAnswerAsync(string userId, int askId, string content);
        Task<IEnumerable<QuestionAnswerDto>> GetAnswersAsync(int askId);
    }
}
