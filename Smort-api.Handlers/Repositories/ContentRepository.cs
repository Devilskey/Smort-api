using Dapper;
using MySql.Data.MySqlClient;
using Smort_api.Object.Videos;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    public class ContentRepository : IContentRepository
    {
        private readonly IDbConnection _db;

        public ContentRepository(MySqlConnection connection)
        {
            _db = connection;
        }

        public async Task<IEnumerable<object>> GetContentListAsync(string? userId, string search, int page = 0)
        {
            // Simplified: use similar SQL as ContentHandler
            if (string.IsNullOrEmpty(userId))
            {
                const string sql = @"
                    SELECT Content.Id, Content.Description, Content.User_Id,
                    (SELECT Id FROM File_Content WHERE Content_Id=Content.Id) As File_Id,
                    Content.Created_At, Users_Public.Username, Content.Type,
                    (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like') AS Likes,
                    null AS AlreadyLiked
                    FROM Content
                    INNER JOIN Users_Public On Content.User_Id = Users_Public.Id 
                    WHERE LOWER(Content.Description) LIKE @asked
                    ORDER BY Content.Created_At DESC LIMIT @max OFFSET @offset;";

                return await _db.QueryAsync<object>(sql, new { asked = $"%{search.ToLower()}%", max = 30, offset = page * 30 });
            }
            else
            {
                const string sql = @"
                    SELECT Content.Id, Content.Description, Content.User_Id, Content.Created_At, Users_Public.Username, Content.Type,
                    (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like') AS Likes,
                    (SELECT EXISTS(SELECT Id FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like' AND User_Id=@user)) AS AlreadyLiked,
                    (SELECT Id FROM File_Content WHERE Content_Id=Content.Id) As File_Id
                    FROM Content
                    INNER JOIN Users_Public On Content.User_Id=Users_Public.Id
                    WHERE LOWER(Content.Description) LIKE @asked
                    ORDER BY Content.Created_At DESC LIMIT @max OFFSET @offset;";

                return await _db.QueryAsync<object>(sql, new { asked = $"%{search.ToLower()}%", user = userId, max = 30, offset = page * 30 });
            }
        }

        public async Task<object?> GetContentFromIdAsync(string? userId, int contentId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                const string sql = @"
                    SELECT Content.Id, Content.Description, Content.User_Id, Content.Created_At, Users_Public.Username, Content.Type,
                    (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like') AS Likes,
                    (SELECT Id FROM File_Content WHERE Content_Id=Content.Id) As File_Id,
                    null AS AlreadyLiked
                    FROM Content 
                    INNER JOIN Users_Public On Content.User_Id = Users_Public.Id 
                    WHERE Content.id = @Contentid;";

                return await _db.QueryFirstOrDefaultAsync<object>(sql, new { Contentid = contentId });
            }
            else
            {
                const string sql = @"
                    SELECT Content.Id, Content.Description, Content.User_Id, Content.Created_At, Users_Public.Username, Content.Type,
                    (SELECT COUNT(Id) FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like') AS Likes,
                    (SELECT Id FROM File_Content WHERE Content_Id=Content.Id) As File_Id,
                    (SELECT EXISTS(SELECT Id FROM Reaction WHERE Content_Id = Content.Id AND Reaction = 'Like' AND User_Id=@user)) AS AlreadyLiked
                    FROM Content
                    INNER JOIN Users_Public On Content.User_Id = Users_Public.Id
                    WHERE Content.id = @Contentid;";

                return await _db.QueryFirstOrDefaultAsync<object>(sql, new { user = userId, Contentid = contentId });
            }
        }

        public async Task<IEnumerable<FilePathData>> GetFilePathsForVideoAsync(int videoId)
        {
            const string sql = @"
                SELECT File_Location FROM File_Content WHERE Content_Id=@VideoId UNION 
                SELECT File_Location FROM File_Image WHERE Id=(SELECT Thumbnail FROM Content WHERE id=@VideoId);";
            return (await _db.QueryAsync<FilePathData>(sql, new { VideoId = videoId })).ToList();
        }

        public async Task DeleteVideoRecordsAsync(int videoId, string userId)
        {
            const string sql1 = "DELETE FROM File_Content WHERE Content_Id= @VideoId;";
            const string sql2 = "DELETE FROM Content WHERE Id = @VideoId AND User_Id = @UserId;";
            const string sql3 = "DELETE FROM File_Image WHERE Id In (SELECT Thumbnail FROM Content WHERE Id = @VideoId);";

            await _db.ExecuteAsync(sql1, new { VideoId = videoId });
            await _db.ExecuteAsync(sql2, new { VideoId = videoId, UserId = userId });
            await _db.ExecuteAsync(sql3, new { VideoId = videoId });
        }

        public async Task<string?> GetVideoFilePathAsync(int videoId)
        {
            const string sql = "SELECT File_Location FROM File_Content WHERE Content_Id=(SELECT Id FROM Content WHERE Id=@Id);";
            return await _db.ExecuteScalarAsync<string?>(sql, new { Id = videoId });
        }

        public async Task<IEnumerable<object>> GetAccountContentListAsync(string id)
        {
            const string sql = @"
                SELECT Content.Id, Content.Thumbnail, Content.Type, File_Content.Id as File_Id, Content.Description
                FROM Content LEFT JOIN File_Content ON  Content.Id=File_Content.Content_Id 
                WHERE User_Id=@Id ";
            return await _db.QueryAsync<object>(sql, new { Id = id });
        }
    }
}
