using Smort_api.Object.Videos;
using System.Threading.Tasks;
using Tiktok_api.BackgroundServices;
using Smort_api.Object.DTO;
using Tiktok_api.Settings_Api;

namespace Tiktok_api.Services
{
    public interface IVideoService
    {
        Task<string> UploadVideoAsync(string userId, VideoUploadData data, ProcessVideoServices videoProcessor);
        Task DeleteVideoAsync(int videoId, string userId);
        Task<IEnumerable<VideoDto>> GetVideoFromIdAsync(string? userId, int id);
        Task<string?> GetVideoFilePathAsync(int videoId, Sizes size);
    }
}
