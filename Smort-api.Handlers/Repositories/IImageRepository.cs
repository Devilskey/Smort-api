using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    public interface IImageRepository
    {
        Task<int?> GetProfilePictureFileIdAsync(int userId);
        Task<string?> GetFilePathByIdAsync(int fileId, bool isContent);
    }
}
