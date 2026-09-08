using System.Threading.Tasks;
using Smort_api.Object.DTO;

namespace Tiktok_api.Services
{
    public interface IReactionsService
    {
        Task<ReactionToggleDto> ToggleLikeAsync(string userId, string contentId, string contentType);
    }
}
