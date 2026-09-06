using Smort_api.Object.Videos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    public interface IVideoRepository
    {
        Task<IEnumerable<object>> GetVideoByIdAsync(string? userId, int id);
        Task<IEnumerable<FilePathData>> GetVideoFilePathsAsync(int videoId);
        Task DeleteVideoAsync(int videoId, string userId);
        Task<string?> GetVideoStreamPathAsync(int videoId);
    }
}
