using Smort_api.Object.Videos;
using Smort_api.Object.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tiktok_api.Services
{
    public interface IContentService
    {
        Task<IEnumerable<ContentItemDto>> GetContentListAsync(string? userId, string search, int page = 0);
        Task<ContentItemDto?> GetContentFromIdAsync(string? userId, int contentId);
        Task<IEnumerable<FilePathData>> GetFilePathsForVideoAsync(int videoId);
        Task DeleteVideoAsync(int videoId, string userId);
        Task<string?> GetVideoFilePathAsync(int videoId);
        Task<IEnumerable<ContentItemDto>> GetAccountContentListAsync(string id);
    }
}
