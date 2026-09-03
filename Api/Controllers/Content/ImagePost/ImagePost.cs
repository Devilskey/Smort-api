using System.Data;
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
using Dapper;
using Tiktok_api.Settings_Api;

namespace Tiktok_api.Controllers.Content.ImagePost
{
    [ApiController]
    public class ImagePost : ControllerBase
    {
        private ILogger<ImagePost> _logger;
        private ChunkHandler chunkHandler;
        private readonly IDbConnection _db;

        public ImagePost(ILogger<ImagePost> logger, IDbConnection db)
        {
            _logger = logger;
            _db = db; 
            chunkHandler = new ChunkHandler("./TempImagePost", "./ImagePost", "webp");
        }

        [Authorize]
        [HttpPost]
        [Route("ImagePosts/CreateNewPost")]
        public async Task<IActionResult> CreateNewPost(CreateNewPostData data)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string id = User.FindFirstValue("app_user_id");

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
                
                var sqlFileAndPostImage = @"
                    INSERT INTO Content (User_Id, Type, Description, Created_At, Updated_At, Deleted_At) 
                    VALUES (@Id,  @Type, @Description, @CreatedAt, @UpdatedAt, @DeletedAt);
                    INSERT INTO File_Content (File_Name, Content_Id, File_location, file_type_Id, Created_At, Deleted_At) 
                    VALUES (@FileName, LAST_INSERT_ID(), @FileLocation, @FileType, @CreatedAt, @DeletedAt)";
                
                await _db.QueryAsync(sqlFileAndPostImage, 
                    new  {
                    FileName= $"{filename}",
                    Id = id,
                    FileLocation = $"./ImagePost/{id}/{filename}/{filename}",
                    FileType=FileType.PostImage,
                    CreatedAt = DateTime.Now,
                    DeletedAt = DateTime.Now,
                    UpdatedAt =DateTime.Now,
                    Type = "img",
                    Description =data.Description
                });
                
                Array.Clear(filePost);

                chunkHandler.TempFileCleanup($"{data.GUIDObjSender}-$", (int)(data.TotalChunks - 1));
                
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
        public async Task<ActionResult> DeleteImage(int imageId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return BadRequest();

            string id = User.FindFirstValue("app_user_id");
            
            var sqlSelectImagePath =
            "SELECT File_Location FROM File_Content WHERE Content_Id IN (SELECT Id FROM Content WHERE Id = @ImageId); " +
            "DELETE FROM File_Content WHERE Content_Id IN (SELECT Id FROM Content WHERE Id = @ImageId);" +
            "DELETE FROM Content WHERE Id = @ImageId AND User_Id = @UserId; ";
            
            var paths = await _db.QueryAsync<FilePathData>(sqlSelectImagePath, new {imageId=imageId, UserId=id});
                
            if (paths.Any())
                return BadRequest();

            foreach (FilePathData path in paths)
            { 
                System.IO.File.Delete(path.File_Location!);
            }
            
            return Ok();
        }
    }
}