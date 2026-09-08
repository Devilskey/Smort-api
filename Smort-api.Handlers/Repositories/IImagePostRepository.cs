using Smort_api.Object.ImagePosts;
using Smort_api.Object.Videos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    public interface IImagePostRepository
    {
        Task CreateImagePostAsync(string userId, string description, string fileName, string fileLocation);
        Task<IEnumerable<FilePathData>> GetImagePathsForDeleteAsync(int imageId, string userId);
        Task DeleteImagePostAsync(int imageId, string userId);
    }
}
