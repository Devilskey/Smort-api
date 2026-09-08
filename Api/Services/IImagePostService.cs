using Smort_api.Object.ImagePosts;
using System.Threading.Tasks;

namespace Tiktok_api.Services
{
    public interface IImagePostService
    {
        Task<string> SaveImagePostAsync(string userId, CreateNewPostData data);
        Task DeleteImageAsync(int imageId, string userId);
    }
}
