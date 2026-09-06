using Smort_api.Handlers.Repositories;
using Smort_api.Object.DTO;
using System.Threading.Tasks;

namespace Tiktok_api.Services
{
    public class ReactionsService : IReactionsService
    {
        private readonly IReactionsRepository _repo;

        public ReactionsService(IReactionsRepository repo)
        {
            _repo = repo;
        }

        public async Task<ReactionToggleDto> ToggleLikeAsync(string userId, string contentId, string contentType)
        {
            var count = await _repo.GetReactionCountAsync(userId, contentId, "Like", contentType);
            if (count == 0)
            {
                await _repo.AddReactionAsync(userId, contentId, contentType, "Like");
                var owner = await _repo.GetContentOwnerAsync(contentId);
                return new ReactionToggleDto
                {
                    TypeOfLike = "Like",
                    Owner = owner.HasValue ? new ReactionOwnerDto { Id = owner.Value.Id, Username = owner.Value.Username } : null
                };
            }
            else
            {
                await _repo.RemoveReactionAsync(userId, contentId, contentType, "Like");
                return new ReactionToggleDto { TypeOfLike = "RemoveLike", Owner = null };
            }
        }
    }
}
