using System.Threading.Tasks;

namespace Tiktok_api.Services
{
    public interface IReactionsService
    {
        Task<(string TypeOfLike, (int Id, string Username)? Owner)> ToggleLikeAsync(string userId, string contentId, string contentType);
    }
}
