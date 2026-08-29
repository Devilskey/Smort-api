using Dapper;
using MySql.Data.MySqlClient;
using Smort_api.Object.Content;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    public class ReactionsRepository : IReactionsRepository
    {
        private readonly IDbConnection _db;

        public ReactionsRepository(MySqlConnection connection)
        {
            _db = connection;
        }

        public async Task<int> GetReactionCountAsync(string userId, string contentId, string reaction, string type)
        {
            const string sql = "SELECT COUNT(Id) FROM Reaction WHERE User_Id=@user AND Content_Id=@content AND Reaction=@reaction AND Content_Type=@type;";
            return await _db.ExecuteScalarAsync<int>(sql, new { user = userId, content = contentId, reaction, type });
        }

        public async Task AddReactionAsync(string userId, string contentId, string type, string reaction)
        {
            const string sql = "INSERT INTO Reaction (User_Id, Content_Id, Content_Type, Reaction, Created_At) VALUES (@user, @content, @type, @reaction, @Created_At);";
            await _db.ExecuteAsync(sql, new { user = userId, content = contentId, type, reaction, Created_At = System.DateTime.UtcNow });
        }

        public async Task RemoveReactionAsync(string userId, string contentId, string type, string reaction)
        {
            const string sql = "DELETE FROM Reaction WHERE User_Id=@user AND Content_Id=@content AND Reaction=@reaction AND Content_Type=@type;";
            await _db.ExecuteAsync(sql, new { user = userId, content = contentId, reaction, type });
        }

        public async Task<(int Id, string Username)?> GetContentOwnerAsync(string contentId)
        {
            const string sql = "SELECT Id, Username FROM Users_Public WHERE Id=(SELECT User_Id FROM Content WHERE id=@content);";
            var result = await _db.QueryFirstOrDefaultAsync<ContentOwner>(sql, new { content = contentId });
            return result == null ? null : (result.Id, result.Username);
        }
    }
}
