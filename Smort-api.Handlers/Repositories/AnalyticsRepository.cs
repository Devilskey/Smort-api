using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly IDbConnection _db;

        public AnalyticsRepository(MySqlConnection connection)
        {
            _db = connection;
        }

        public async Task<bool> PageExistsAsync(string pageName)
        {
            const string sql = "SELECT COUNT(*) FROM Pages WHERE Name=@PageName;";
            var count = await _db.ExecuteScalarAsync<int>(sql, new { PageName = pageName });
            return count > 0;
        }

        public async Task<int?> GetPageIdAsync(string pageName)
        {
            const string sql = "SELECT Id FROM Pages WHERE Name=@PageName;";
            return await _db.ExecuteScalarAsync<int?>(sql, new { PageName = pageName });
        }

        public async Task IncrementMonthlyViewAsync(int pageId)
        {
            const string sql = "UPDATE Page_Views_Monthly SET ViewCount = ViewCount + 1 WHERE Page_Id=@PageId;";
            await _db.ExecuteAsync(sql, new { PageId = pageId });
        }

        public async Task<int> CreatePageAsync(string pageName)
        {
            const string sql = "INSERT INTO Pages (Name) VALUES (@PageName); SELECT LAST_INSERT_ID();";
            return await _db.ExecuteScalarAsync<int>(sql, new { PageName = pageName });
        }
    }
}
