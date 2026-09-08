using Smort_api.Handlers;
using Smort_api.Handlers.Repositories;
using Smort_api.Object.ImagePosts;
using Smort_api.Object.Videos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tiktok_api.Settings_Api;

namespace Tiktok_api.Services
{
    public class ImagePostService : IImagePostService
    {
        private readonly IImagePostRepository _imagePostRepository;
        private readonly ChunkHandler _chunkHandler;

        public ImagePostService(IImagePostRepository imagePostRepository)
        {
            _imagePostRepository = imagePostRepository;
            _chunkHandler = new ChunkHandler("./TempImagePost", "./ImagePost", "webp");
        }

        public async Task<string> SaveImagePostAsync(string userId, CreateNewPostData data)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User id is required.", nameof(userId));

            if (data == null)
                throw new ArgumentException("Image data is required.", nameof(data));

            string filename = $"{data.GUIDObjSender}-{data.ChunkNumber}";
            _chunkHandler.SaveFileChunk(data.MediaData, filename);

            if (!_chunkHandler.AreAllChunksIn($"{data.GUIDObjSender}-$", (int)(data.TotalChunks - 1)))
            {
                return "Chunk Saved";
            }

            byte[] filePost = Array.Empty<byte>();
            for (int i = 0; i < data.TotalChunks; i++)
            {
                var tempFileName = $"{data.GUIDObjSender}-{i}";
                filePost = filePost.Concat(_chunkHandler.GetChunkFileData(tempFileName)).ToArray();
            }

            Guid fileSavedId = Guid.NewGuid();
            string finalFilename = fileSavedId.ToString();

            foreach (var size in ContentSizingObjects.Content)
            {
                float percentageLesser = (float)size.Width / (float)data.size.Width;

                if (percentageLesser == 0)
                {
                    percentageLesser = (float)data.size.Width / (float)size.Width;
                }

                int newWidth = (int)(percentageLesser * data.size.Width);
                int newHeight = (int)(percentageLesser * data.size.Height);

                var resizedFilePost = ImageHandler.ChangeSizeOfImage(filePost, newWidth, newHeight);
                if (resizedFilePost != null)
                {
                    _chunkHandler.SaveFile(resizedFilePost, finalFilename, userId, $"_{size.Size}");
                }
            }

            string thumbnailFilename = finalFilename + "_Thumb";
            foreach (var size in ContentSizingObjects.Thumbnails)
            {
                float percentageLesser = (float)size.Width / (float)data.size.Width;

                if (percentageLesser == 0)
                {
                    percentageLesser = (float)data.size.Width / (float)size.Width;
                }

                int newWidth = (int)(percentageLesser * data.size.Width);
                int newHeight = (int)(percentageLesser * data.size.Height);

                var resizedFilePost = ImageHandler.ChangeSizeOfImage(filePost, newWidth, newHeight);
                if (resizedFilePost != null)
                {
                    _chunkHandler.SaveFile(resizedFilePost, thumbnailFilename, userId, $"_{size.Size}");
                }
            }

            await _imagePostRepository.CreateImagePostAsync(
                userId,
                data.Description,
                finalFilename,
                $"./ImagePost/{userId}/{finalFilename}/{finalFilename}");

            Array.Clear(filePost);
            _chunkHandler.TempFileCleanup($"{data.GUIDObjSender}-$", (int)(data.TotalChunks - 1));

            return "Saved the new Post";
        }

        public async Task DeleteImageAsync(int imageId, string userId)
        {
            if (imageId <= 0)
                throw new ArgumentException("A valid image id is required.", nameof(imageId));

            var paths = (await _imagePostRepository.GetImagePathsForDeleteAsync(imageId, userId)).ToList();

            if (!paths.Any())
                throw new InvalidOperationException("Image not found or not owned by user.");

            foreach (var path in paths)
            {
                if (!string.IsNullOrWhiteSpace(path.File_Location))
                {
                    if (File.Exists(path.File_Location))
                    {
                        File.Delete(path.File_Location);
                    }
                }
            }

            await _imagePostRepository.DeleteImagePostAsync(imageId, userId);
        }
    }
}
