using Smort_api.Handlers;
using Smort_api.Handlers.Repositories;
using Smort_api.Object.Videos;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Smort_api.Object.Video;
using Tiktok_api.BackgroundServices;
using Tiktok_api.Settings_Api;

namespace Tiktok_api.Services
{
    public class VideoService : IVideoService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly ChunkHandler _chunkHandler;

        public VideoService(IVideoRepository videoRepository)
        {
            _videoRepository = videoRepository;
            _chunkHandler = new ChunkHandler("./TempVideos", "./Videos", "mkv");
        }

        public async Task<string> UploadVideoAsync(string userId, VideoUploadData data, ProcessVideoServices videoProcessor)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User id is required.", nameof(userId));

            if (data == null || data.MediaData == null || data.ChunkNumber == null || data.TotalChunks == null)
                throw new ArgumentException("Missing upload data.", nameof(data));

            data.FileName = $"{data.GUIDObjSender}-{data.ChunkNumber}";
            _chunkHandler.SaveFileChunk(data.MediaData, data.FileName);
            
            Console.WriteLine(data.TotalChunks);

            if (!_chunkHandler.AreAllChunksIn($"{data.GUIDObjSender}-", (int)(data.TotalChunks - 1)))
            {
                return "Chunk Saved";
            }

            byte[] videoBytes = Array.Empty<byte>();
            for (int i = 0; i < data.TotalChunks; i++)
            {
                var tempFileName = $"{data.GUIDObjSender}-{i}";
                videoBytes = videoBytes.Concat(_chunkHandler.GetChunkFileData(tempFileName)).ToArray();
            }

            Guid videoSavedId = Guid.NewGuid();
            data.FileName = videoSavedId.ToString();

            _chunkHandler.SaveFileChunk(videoBytes, data.FileName + "TS");

            string input = _chunkHandler.GetPathContentTemp(data.FileName + "TS");
            string output = _chunkHandler.GetPath(data.FileName, userId);

            videoProcessor.AddToQueue(new VideoToProcessObject
            {
                Output = output,
                Input = input,
                Description = data.Description,
                UserId = userId,
                FileName = data.FileName
            });

            Array.Clear(videoBytes);
            _chunkHandler.TempFileCleanup($"{data.GUIDObjSender}-$", (int)(data.TotalChunks - 1));

            return "Saved the new Post";
        }

        public async Task DeleteVideoAsync(int videoId, string userId)
        {
            if (videoId <= 0)
                throw new ArgumentException("A valid video id is required.", nameof(videoId));

            var paths = (await _videoRepository.GetVideoFilePathsAsync(videoId)).ToList();
            if (!paths.Any())
                throw new InvalidOperationException("Video not found or not owned by user.");

            foreach (var path in paths)
            {
                if (!string.IsNullOrEmpty(path.File_Location) && File.Exists(path.File_Location))
                    File.Delete(path.File_Location);
            }

            await _videoRepository.DeleteVideoAsync(videoId, userId);
        }

        public async Task<object> GetVideoFromIdAsync(string? userId, int id)
        {
            return await _videoRepository.GetVideoByIdAsync(userId, id);
        }

        public async Task<string?> GetVideoFilePathAsync(int videoId, Sizes size)
        {
            var path = await _videoRepository.GetVideoStreamPathAsync(videoId);
            if (string.IsNullOrWhiteSpace(path))
                return null;

            return path + $"_{size}.mp4";
        }
    }
}
