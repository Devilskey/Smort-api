using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    public class AskMeRepository : IAskMeRepository
    {
        private readonly IDbConnection _db;

        public AskMeRepository(MySqlConnection connection)
        {
            _db = connection;
        }

        public async Task CreateQuestionAsync(string userId, string content)
        {
            const string sql = @"
                INSERT INTO Content (User_Id, Type, Description, Created_At, Updated_At, Deleted_At) 
                VALUES (@UserId, @Type, @Description, @CreatedAt, @UpdatedAt, @DeletedAt);";

            await _db.ExecuteAsync(sql, new
            {
                UserId = userId,
                Type = "Ask",
                Description = content,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                DeletedAt = DateTime.Now
            });
        }

        public async Task CreateAnswerAsync(string userId, int askId, string content)
        {
            const string sql = @"
                INSERT INTO Content_Answer (User_Id, Content_Id, Answer, Created_At, Updated_At) 
                VALUES (@UserId, @AskId, @Answer, @CreatedAt, @UpdatedAt);";

            await _db.ExecuteAsync(sql, new
            {
                UserId = userId,
                AskId = askId,
                Answer = content,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        public async Task<IEnumerable<object>> GetAnswersByQuestionIdAsync(int askId)
        {
            const string sql = @"
                SELECT User_Id, Answer 
                FROM Content_Answer 
                WHERE Content_Id = @AskId;";

            return await _db.QueryAsync<object>(sql, new { AskId = askId });
        }
    }
}
