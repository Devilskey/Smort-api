using Smort_api.Object.Videos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tiktok_api.Services
{
    public interface IContentService
    {
        Task<IEnumerable<object>> GetContentListAsync(string? userId, string search, int page = 0);
        Task<object?> GetContentFromIdAsync(string? userId, int contentId);
        Task<IEnumerable<FilePathData>> GetFilePathsForVideoAsync(int videoId);
        Task DeleteVideoAsync(int videoId, string userId);
        Task<string?> GetVideoFilePathAsync(int videoId);
        Task<IEnumerable<object>> GetAccountContentListAsync(string id);
    }
}
