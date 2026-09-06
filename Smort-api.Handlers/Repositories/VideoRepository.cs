using Dapper;
using MySql.Data.MySqlClient;
using Smort_api.Object.Videos;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly IDbConnection _db;

        public VideoRepository(MySqlConnection connection)
        {
            _db = connection;
        }

        public async Task<IEnumerable<object>> GetVideoByIdAsync(string? userId, int id)
        {
            const string sqlLoggedIn = @"
                SELECT Id, Description, Created_At, User_Id,
                    (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like' AND Content_Type = 'vid') AS Likes,
                    (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like' AND Content_Type = 'vid' AND User_Id = @UserId) AS AlreadyLiked
                FROM Content
                WHERE Id = @Id AND Type = 'vid';";

            const string sqlAnonymous = @"
                SELECT Id, Description, Created_At, User_Id,
                    (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like' AND Content_Type = 'vid') AS Likes,
                    NULL AS AlreadyLiked
                FROM Content
                WHERE Id = @Id AND Type = 'vid';";

            var sql = string.IsNullOrWhiteSpace(userId) ? sqlAnonymous : sqlLoggedIn;
            return await _db.QueryAsync<object>(sql, new { UserId = userId, Id = id });
        }

        public async Task<IEnumerable<FilePathData>> GetVideoFilePathsAsync(int videoId)
        {
            const string sql = @"
                SELECT File_Location FROM File_Content WHERE Content_Id = @VideoId
                UNION
                SELECT File_Location FROM File_Image WHERE Id = (SELECT Thumbnail FROM Content WHERE id = @VideoId);";

            return await _db.QueryAsync<FilePathData>(sql, new { VideoId = videoId });
        }

        public async Task DeleteVideoAsync(int videoId, string userId)
        {
            const string sql = @"
                DELETE FROM File_Content WHERE Content_Id = @VideoId;
                DELETE FROM Content WHERE Id = @VideoId AND User_Id = @UserId;
                DELETE FROM File_Image WHERE Id IN (SELECT Thumbnail FROM Content WHERE Id = @VideoId);";

            await _db.ExecuteAsync(sql, new { VideoId = videoId, UserId = userId });
        }

        public async Task<string?> GetVideoStreamPathAsync(int videoId)
        {
            const string sql = "SELECT File_Location FROM File_Content WHERE Content_Id=(SELECT Id FROM Content WHERE Id=@Id);";
            return await _db.ExecuteScalarAsync<string?>(sql, new { Id = videoId });
        }
    }
}
