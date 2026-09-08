using Smort_api.Handlers.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Smort_api.Object.DTO;

namespace Tiktok_api.Services
{
    public class AskMeService : IAskMeService
    {
        private readonly IAskMeRepository _askMeRepository;

        public AskMeService(IAskMeRepository askMeRepository)
        {
            _askMeRepository = askMeRepository;
        }

        public async Task CreateQuestionAsync(string userId, string content)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User id is required.", nameof(userId));

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Question content is required.", nameof(content));

            await _askMeRepository.CreateQuestionAsync(userId, content);
        }

        public async Task CreateAnswerAsync(string userId, int askId, string content)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User id is required.", nameof(userId));

            if (askId <= 0)
                throw new ArgumentException("A valid ask id is required.", nameof(askId));

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Answer content is required.", nameof(content));

            await _askMeRepository.CreateAnswerAsync(userId, askId, content);
        }

        public async Task<IEnumerable<QuestionAnswerDto>> GetAnswersAsync(int askId)
        {
            if (askId <= 0)
                throw new ArgumentException("A valid ask id is required.", nameof(askId));

            return await _askMeRepository.GetAnswersByQuestionIdAsync(askId);
        }
    }
}
