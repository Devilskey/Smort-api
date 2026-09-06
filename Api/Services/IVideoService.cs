using Smort_api.Object.Videos;
using System.Threading.Tasks;
using Tiktok_api.BackgroundServices;
using Tiktok_api.Settings_Api;

namespace Tiktok_api.Services
{
    public interface IVideoService
    {
        Task<string> UploadVideoAsync(string userId, VideoUploadData data, ProcessVideoServices videoProcessor);
        Task DeleteVideoAsync(int videoId, string userId);
        Task<object> GetVideoFromIdAsync(string? userId, int id);
        Task<string?> GetVideoFilePathAsync(int videoId, Sizes size);
    }
}
