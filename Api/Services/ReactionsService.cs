using Smort_api.Handlers.Repositories;
using Smort_api.Object.DTO;
using System.Threading.Tasks;
using Smort_api.Handlers;

namespace Tiktok_api.Services
{
    public class ReactionsService : IReactionsService
    {
        private readonly IReactionsRepository _repo;
        private readonly NotificationsHandler _notifcations;


        public ReactionsService(IReactionsRepository repo, NotificationsHandler notifcations )
        {
            _repo = repo;
            _notifcations = notifcations;;
        }

        public async Task<ReactionToggleDto> ToggleLikeAsync(string userId, string contentId, string contentType)
        {
            var count = await _repo.GetReactionCountAsync(userId, contentId, "Like", contentType);
            if (count == 0)
            {
                await _repo.AddReactionAsync(userId, contentId, contentType, "Like");
                var owner = await _repo.GetContentOwnerAsync(contentId);
                _notifcations.SendNotification(owner.Value.Id.ToString(), "New Follower", "Liked your content");
                
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
