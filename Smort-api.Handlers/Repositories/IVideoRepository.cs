using Smort_api.Object.Videos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Smort_api.Object.DTO;

namespace Smort_api.Handlers.Repositories
{
    public interface IVideoRepository
    {
        Task<IEnumerable<VideoDto>> GetVideoByIdAsync(string? userId, int id);
        Task<IEnumerable<FilePathData>> GetVideoFilePathsAsync(int videoId);
        Task DeleteVideoAsync(int videoId, string userId);
        Task<string?> GetVideoStreamPathAsync(int videoId);
    }
}
