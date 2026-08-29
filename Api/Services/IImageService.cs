using System.Threading.Tasks;

namespace Tiktok_api.Services
{
    public interface IImageService
    {
        Task<string?> GetProfileImagePathAsync(int userId);
        Task<string?> GetImagePathAsync(int imageId, bool isContent);
    }
}
