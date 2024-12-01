using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object;
using Smort_api.Object.Database;
using Smort_api.Object.ImagePosts;
using Smort_api.Object.Videos;
using System.Security.Claims;
using Tiktok_api.Controllers.Analytics;
using static System.Net.Mime.MediaTypeNames;

namespace Tiktok_api.Controllers.ImagePost
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
            using (var databaseHandler = new DatabaseHandler())
            {
                MySqlCommand GetsearchResult = new MySqlCommand();
                GetsearchResult.CommandText =
                    " SELECT Image_Post.Title, Image_Post.Description,  Image_Post.File_Id, Image_Post.User_Id, Users_Public.Username" +
                    " FROM Image_Post" +
                    " INNER JOIN Users_Public ON Image_Post.User_Id = Users_Public.Id" +
                    " WHERE Image_Post.Title LIKE @search OR Image_Post.Description LIKE @search" +
                    " ORDER BY RAND() LIMIT 10; ";

                GetsearchResult.Parameters.AddWithValue("@search", $"%{Search}%");

                string json = databaseHandler.Select(GetsearchResult);

                return Task.FromResult(json);
            }
        }


        [HttpGet]
        [Route("ImagePosts/GetImagePosts")]
        public Task<string> GetImagePost()
        {
            using (var databaseHandler = new DatabaseHandler())
            {
                MySqlCommand getPostList = new MySqlCommand();
                getPostList.CommandText = 
                    "SELECT Image_Post.Title,  Image_Post.Description,  Image_Post.File_Id, Image_Post.User_Id, Users_Public.Username " +
                    "FROM Image_Post INNER JOIN Users_Public ON Image_Post.User_Id = Users_Public.Id ORDER BY RAND() LIMIT 10; ";
                string json = databaseHandler.Select(getPostList);

                return Task.FromResult(json);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("ImagePosts/CreateNewPost")]
        public Task<string> CreateNewPost(CreateNewPostData data)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Task.FromResult("token is blacklisted");

            string id = User.FindFirstValue("Id");

            string filename = $"{data.GUIDObjSender}-${data.ChunkNumber}";

            chunkHandler.SaveFileChunk(data.MediaData, filename);

            if (chunkHandler.AreAllChunksIn($"{data.GUIDObjSender}-$", (int)(data.TotalChunks - 1)))
            {
                byte[] filePost = new byte[0];

                for (int i = 0; i < data.TotalChunks; i++)
                {
                    var tempFileName = $"{data.GUIDObjSender}-${i}";
                    filePost = filePost.Concat(chunkHandler.GetChunkFileData(tempFileName)).ToArray();
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
            }
            else
            {
                data = null;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return Task.FromResult("Post Saved");
        }

        [Route("ImagePosts/GetImageFromId")]
        [HttpGet]
        public Task<string> GetImageFromId(int id)
        {
            using MySqlCommand GetImage = new MySqlCommand();
            GetImage.CommandText = "SELECT Id, Title, Description, File_Id, Created_At FROM Image_Post WHERE Id=@Id;";
            GetImage.Parameters.AddWithValue("@Id", id);
            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                string json = databaseHandler.Select(GetImage);
                return Task.FromResult(json);
            }
        }
    }
}
