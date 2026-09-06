using Smort_api.Handlers.Repositories;
using Smort_api.Object.Videos;
using Smort_api.Object.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tiktok_api.Services
{
    public class ContentService : IContentService
    {
        private readonly IContentRepository _repo;

        public ContentService(IContentRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ContentItemDto>> GetContentListAsync(string? userId, string search, int page = 0)
        {
            return await _repo.GetContentListAsync(userId, search, page);
        }

        public async Task<ContentItemDto?> GetContentFromIdAsync(string? userId, int contentId)
        {
            return await _repo.GetContentFromIdAsync(userId, contentId);
        }

        public async Task<IEnumerable<FilePathData>> GetFilePathsForVideoAsync(int videoId)
        {
            return await _repo.GetFilePathsForVideoAsync(videoId);
        }

        public async Task DeleteVideoAsync(int videoId, string userId)
        {
            await _repo.DeleteVideoRecordsAsync(videoId, userId);
        }

        public async Task<string?> GetVideoFilePathAsync(int videoId)
        {
            return await _repo.GetVideoFilePathAsync(videoId);
        }

        public async Task<IEnumerable<ContentItemDto>> GetAccountContentListAsync(string id)
        {
            return await _repo.GetAccountContentListAsync(id);
        }
    }
}
