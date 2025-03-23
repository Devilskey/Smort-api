using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Smort_api.Handlers;
using Smort_api.Object.Database;
using Smort_api.Object.ImagePosts;
using Smort_api.Object.Videos;
using System.Security.Claims;
using Tiktok_api.Settings_Api;

namespace Tiktok_api.Controllers.Content.ImagePost
{
    [ApiController]
    public class ImagePost : ControllerBase
    {
        private ILogger<ImagePost> _logger;
        private ChunkHandler chunkHandler;
        public ImagePost(ILogger<ImagePost> logger)
        {
            _logger = logger;
            chunkHandler = new ChunkHandler("./TempImagePost", "./ImagePost", "png");
        }

        [Authorize]
        [HttpPost]
        [Route("ImagePosts/CreateNewPost")]
        public IActionResult CreateNewPost(CreateNewPostData data)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string id = User.FindFirstValue("Id");

            string filename = $"{data.GUIDObjSender}-${data.ChunkNumber}";

            chunkHandler.SaveFileChunk(data.MediaData, filename);

            if (chunkHandler.AreAllChunksIn($"{data.GUIDObjSender}-$", (int)(data.TotalChunks - 1)))
            {
                // Get all chunks
                byte[] filePost = new byte[0];

                for (int i = 0; i < data.TotalChunks; i++)
                {
                    var tempFileName = $"{data.GUIDObjSender}-${i}";
                    filePost = filePost.Concat(chunkHandler.GetChunkFileData(tempFileName)).ToArray();
                }

                Guid fileSavedId = Guid.NewGuid();
                filename = fileSavedId.ToString();

                // Resize image for Content
                foreach (var size in ContentSizingObjects.Content) {
                    float percentageLesser = (float)size.Width / (float)data.size.Width;

                    if(percentageLesser == 0)
                    {
                        percentageLesser = (float)data.size.Width / (float)size.Width;
                    }

                    int newWidth = (int)(percentageLesser * data.size.Width);
                    int newHeight = (int)(percentageLesser * data.size.Height);

                    var ResizedFilePost = ImageHandler.ChangeSizeOfImage(filePost, newWidth, newHeight);

                    if (ResizedFilePost != null)
                    {
                        chunkHandler.SaveFile(ResizedFilePost, filename, id, $"_{size.Size}");
                    }
                }

                string Thumbnailfilename = fileSavedId.ToString() + "_Thumb";

                // Resize image for Thumbnail
                foreach (var size in ContentSizingObjects.Thumbnails)
                {
                    float percentageLesser = (float)size.Width / (float)data.size.Width;

                    if (percentageLesser == 0)
                    {
                        percentageLesser = (float)data.size.Width / (float)size.Width;
                    }

                    int newWidth = (int)(percentageLesser * data.size.Width);
                    int newHeight = (int)(percentageLesser * data.size.Height);

                    var ResizedFilePost = ImageHandler.ChangeSizeOfImage(filePost, newWidth, newHeight);

                    if (ResizedFilePost != null)
                    {
                        chunkHandler.SaveFile(ResizedFilePost, Thumbnailfilename, id, $"_{size.Size}");
                    }
                }

                using (var databaseHandler = new DatabaseHandler())
                {
                    using MySqlCommand FileAndPostImage = new MySqlCommand();

                    FileAndPostImage.CommandText =
                        @"INSERT INTO File (File_Name, File_location, file_type_Id, Created_At, Deleted_At) VALUES (@FileName, @FileLocation, @FileType, @CreatedAt, @DeletedAt);
                      INSERT INTO Content (User_Id, File_Id, Title, Type, Description, Created_At, Updated_At, Deleted_At) VALUES (@Id, LAST_INSERT_ID(), @Title, @Type, @Description, @CreatedAt, @UpdatedAt, @DeletedAt);";

                    FileAndPostImage.Parameters.AddWithValue("@FileName", $"{filename}");
                    FileAndPostImage.Parameters.AddWithValue("@Id", id);
                    FileAndPostImage.Parameters.AddWithValue("@FileLocation", $"./ImagePost/{id}/{filename}/{filename}");
                    FileAndPostImage.Parameters.AddWithValue("@FileType", FileType.PostImage);

                    FileAndPostImage.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    FileAndPostImage.Parameters.AddWithValue("@DeletedAt", DateTime.Now);
                    FileAndPostImage.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                    FileAndPostImage.Parameters.AddWithValue("@Title", data.Title);
                    FileAndPostImage.Parameters.AddWithValue("@Type", "img");


                    FileAndPostImage.Parameters.AddWithValue("@Description", data.Description);

                    databaseHandler.EditDatabase(FileAndPostImage);

                    FileAndPostImage.Dispose();

                    Array.Clear(filePost);

                    chunkHandler.TempFileCleanup($"{data.GUIDObjSender}-$", (int)(data.TotalChunks - 1));
                }
                data = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();

                return Ok("Saved the new Post");

            }
            else
            {
                data = null;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return Ok("Chunk Saved");
        }

        [Authorize]
        [Route("ImagePosts/DeleteImage")]
        [HttpDelete]
        public Task<ActionResult> DeleteImage(int imageId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult<ActionResult>(BadRequest());

            string id = User.FindFirstValue("Id");

            using MySqlCommand SelectImagePath = new MySqlCommand();

            SelectImagePath.CommandText =
                "SELECT File_Location FROM File WHERE Id IN (SELECT File_Id FROM Content WHERE Id = @ImageId); " +
                                "DELETE FROM Content WHERE Id = @ImageId AND User_Id = @UserId; " +
            "DELETE FROM File WHERE Id IN (SELECT File_Id FROM Content WHERE Id = @ImageId);";


            SelectImagePath.Parameters.AddWithValue("@ImageId", imageId);
            SelectImagePath.Parameters.AddWithValue("@UserId", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                string json = databaseHandler.Select(SelectImagePath);

                _logger.LogInformation(json);

                FilePathData[] paths = JsonConvert.DeserializeObject<FilePathData[]>(json)!;
                if (paths.Length != 1)
                    return Task.FromResult<ActionResult>(BadRequest());

                foreach (FilePathData path in paths)
                {
                    System.IO.File.Delete(path.File_Location!);
                }
            }
            return Task.FromResult<ActionResult>(Ok());
        }
    }
}