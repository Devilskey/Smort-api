using Dapper;
using MySql.Data.MySqlClient;
using Smort_api.Object.ImagePosts;
using Smort_api.Object.Videos;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Smort_api.Object.Database;

namespace Smort_api.Handlers.Repositories
{
    public class ImagePostRepository : IImagePostRepository
    {
        private readonly IDbConnection _db;

        public ImagePostRepository(MySqlConnection connection)
        {
            _db = connection;
        }

        public async Task CreateImagePostAsync(string userId, string description, string fileName, string fileLocation)
        {
            const string sql = @"
                INSERT INTO Content (User_Id, Type, Description, Created_At, Updated_At, Deleted_At) 
                VALUES (@UserId, @Type, @Description, @CreatedAt, @UpdatedAt, @DeletedAt);
                INSERT INTO File_Content (File_Name, Content_Id, File_location, file_type_Id, Created_At, Deleted_At) 
                VALUES (@FileName, LAST_INSERT_ID(), @FileLocation, @FileType, @CreatedAt, @DeletedAt);";

            await _db.ExecuteAsync(sql, new
            {
                UserId = userId,
                Type = "img",
                Description = description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                DeletedAt = DateTime.Now,
                FileName = fileName,
                FileLocation = fileLocation,
                FileType = FileType.PostImage
            });
        }

        public async Task<IEnumerable<FilePathData>> GetImagePathsForDeleteAsync(int imageId, string userId)
        {
            const string sql = @"
                SELECT File_Location 
                FROM File_Content 
                WHERE Content_Id IN (SELECT Id FROM Content WHERE Id = @ImageId AND User_Id = @UserId);";

            return await _db.QueryAsync<FilePathData>(sql, new { ImageId = imageId, UserId = userId });
        }

        public async Task DeleteImagePostAsync(int imageId, string userId)
        {
            const string sql = @"
                DELETE FROM File_Content WHERE Content_Id IN (SELECT Id FROM Content WHERE Id = @ImageId AND User_Id = @UserId);
                DELETE FROM Content WHERE Id = @ImageId AND User_Id = @UserId;";

            await _db.ExecuteAsync(sql, new { ImageId = imageId, UserId = userId });
        }
    }
}
