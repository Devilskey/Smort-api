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

        [HttpGet]
        [Route("ImagePosts/SearchImagePost")]
        public Task<string> SearchImagePost(string Search)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id");

            using (var databaseHandler = new DatabaseHandler())
            {
                MySqlCommand GetsearchResult = new MySqlCommand();

                if (string.IsNullOrEmpty(id))
                {
                    GetsearchResult.CommandText =
                        " SELECT Image_Post.Id, Image_Post.Title, Image_Post.Description,  Image_Post.File_Id, Image_Post.User_Id, Users_Public.Username, " +
                        " (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \"Like\" AND Content_Type=\"img\") AS Likes" +
                        " FROM Image_Post" +
                        " INNER JOIN Users_Public ON Image_Post.User_Id = Users_Public.Id" +
                        " WHERE Image_Post.Title LIKE @search OR Image_Post.Description LIKE @search" +
                        " ORDER BY RAND() LIMIT 10; ";
                }
                else
                {
                    GetsearchResult.CommandText =
                        " SELECT Image_Post.Id, Image_Post.Title, Image_Post.Description,  Image_Post.File_Id, Image_Post.User_Id, Users_Public.Username, " +
                        " (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \"Like\" AND Content_Type=\"img\") AS Likes," +
                        " (SELECT EXISTS(SELECT Id FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \"Like\" AND Content_Type=\"img\" AND User_Id=@user)) AS AlreadyLiked " +
                        " FROM Image_Post" +
                        " INNER JOIN Users_Public ON Image_Post.User_Id = Users_Public.Id" +
                        " WHERE Image_Post.Title LIKE @search OR Image_Post.Description LIKE @search" +
                        " ORDER BY RAND() LIMIT 10; ";
                    GetsearchResult.Parameters.AddWithValue("@user", id);
                }

                GetsearchResult.Parameters.AddWithValue("@search", $"%{Search}%");

                string json = databaseHandler.Select(GetsearchResult);

                return Task.FromResult(json);
            }
        }


        [HttpGet]
        [Route("ImagePosts/GetImagePosts")]
        public Task<string> GetImagePost()
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id");

            using (var databaseHandler = new DatabaseHandler())
            {
                MySqlCommand getPostList = new MySqlCommand();
                if (string.IsNullOrEmpty(id))
                {
                    getPostList.CommandText =
                    "SELECT Image_Post.Id, Image_Post.Title,  Image_Post.Description,  Image_Post.File_Id, Image_Post.User_Id, Users_Public.Username, " +
                     "(SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \"Like\" AND Content_Type=\"img\") AS Likes " +
                    "FROM Image_Post INNER JOIN Users_Public ON Image_Post.User_Id = Users_Public.Id ORDER BY RAND() LIMIT 10; ";
                }
                else
                {
                    getPostList.CommandText =
                      "SELECT Image_Post.Id, Image_Post.Title,  Image_Post.Description,  Image_Post.File_Id, Image_Post.User_Id, Users_Public.Username, " +
                       "(SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \"Like\" AND Content_Type=\"img\") AS Likes, " +
                        "(SELECT EXISTS(SELECT Id FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \"Like\" AND Content_Type=\"img\" AND User_Id=@user)) AS AlreadyLiked " +
                      "FROM Image_Post INNER JOIN Users_Public ON Image_Post.User_Id = Users_Public.Id ORDER BY RAND() LIMIT 10; ";
                    getPostList.Parameters.AddWithValue("@user", id);
                }
                string json = databaseHandler.Select(getPostList);

                _logger.LogInformation(json);

                return Task.FromResult(json);
            }
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
                Console.WriteLine(data.size.Width);

                if (data.size.Width > 1000)
                {
                    // Resize image
                    float percentageLesser = 1000f / data.size.Width;

                    int newWidth = (int)(percentageLesser * data.size.Width);
                    int newHeight = (int)(percentageLesser * data.size.Width);
                    Console.WriteLine(newHeight);

                    var ResizedFilePost = ImageHandler.ChangeSizeOfImage(filePost, newWidth, newHeight);

                    if (ResizedFilePost != null)
                    {
                        filePost = ResizedFilePost;
                    }
                }

                Guid fileSavedId = Guid.NewGuid();
                filename = fileSavedId.ToString();

                chunkHandler.SaveFile(filePost, filename, id);

                using (var databaseHandler = new DatabaseHandler())
                {
                    using MySqlCommand FileAndPostImage = new MySqlCommand();

                    FileAndPostImage.CommandText =
                        @"INSERT INTO File (File_Name, File_location, file_type_Id, Created_At, Deleted_At) VALUES (@FileName, @FileLocation, @FileType, @CreatedAt, @DeletedAt);
                      INSERT INTO Image_Post (User_Id, File_Id, Title, Description, Created_At, Updated_At, Deleted_At) VALUES (@Id, LAST_INSERT_ID(), @Title, @Description, @CreatedAt, @UpdatedAt, @DeletedAt);";

                    FileAndPostImage.Parameters.AddWithValue("@FileName", $"{filename}.mkv");
                    FileAndPostImage.Parameters.AddWithValue("@Id", id);
                    FileAndPostImage.Parameters.AddWithValue("@FileLocation", $"./ImagePost/{id}/{filename}.png");
                    FileAndPostImage.Parameters.AddWithValue("@FileType", FileType.PostImage);

                    FileAndPostImage.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    FileAndPostImage.Parameters.AddWithValue("@DeletedAt", DateTime.Now);
                    FileAndPostImage.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

                    FileAndPostImage.Parameters.AddWithValue("@Title", data.Title);

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
        public Task<ActionResult> DeleteVideo(int imageId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult<ActionResult>(BadRequest());

            string id = User.FindFirstValue("Id");

            using MySqlCommand SelectImagePath = new MySqlCommand();

            SelectImagePath.CommandText =
                "SELECT File_Location FROM File WHERE Id IN " +
                "(SELECT File_Id FROM Image_Post WHERE Id = @ImageId); " +
                "DELETE FROM Image_Post WHERE Id = @ImageId AND User_Id = @UserId; " +
                "DELETE FROM File WHERE Id IN (SELECT File_Id FROM Image_Post WHERE Id = @ImageId);";

            SelectImagePath.Parameters.AddWithValue("@ImageId", imageId);
            SelectImagePath.Parameters.AddWithValue("@UserId", id);

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                string json = databaseHandler.Select(SelectImagePath);

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

        [Route("ImagePosts/GetImageFromId")]
        [HttpGet]
        public Task<string> GetImageFromId(int id)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string userid = User.FindFirstValue("Id");

            using MySqlCommand GetImage = new MySqlCommand();
            if (string.IsNullOrEmpty(userid))
            {

                GetImage.CommandText =
                    "SELECT Id, Title, Description, File_Id, Created_At, " +
                    "(SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \"Like\" AND Content_Type=\"img\") AS Likes, " +
                    "FROM Image_Post WHERE Id=@Id;";
            }
            else
            {
                GetImage.CommandText =
                    "SELECT Id, Title, Description, File_Id, Created_At, " +
                    "(SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \"Like\" AND Content_Type=\"img\") AS Likes, " +
                    "(SELECT EXISTS(SELECT Id FROM Reaction WHERE Content_Id = Image_Post.Id AND Reaction = \"Like\" AND Content_Type=\"img\" AND User_Id=@user)) AS AlreadyLiked " +
                    "FROM Image_Post WHERE Id=@Id;";
                GetImage.Parameters.AddWithValue("@user", userid);
            }
            GetImage.Parameters.AddWithValue("@Id", id);
            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                string json = databaseHandler.Select(GetImage);
                return Task.FromResult(json);
            }
        }
    }
}