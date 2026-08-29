using Smort_api.Handlers.Repositories;
using System.Threading.Tasks;

namespace Tiktok_api.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _repo;

        public ImageService(IImageRepository repo)
        {
            _repo = repo;
        }

        public async Task<string?> GetProfileImagePathAsync(int userId)
        {
            var fileId = await _repo.GetProfilePictureFileIdAsync(userId);
            if (fileId == null) return null;
            return await _repo.GetFilePathByIdAsync(fileId.Value, isContent: false);
        }

        public async Task<string?> GetImagePathAsync(int imageId, bool isContent)
        {
            return await _repo.GetFilePathByIdAsync(imageId, isContent);
        }
    }
}
