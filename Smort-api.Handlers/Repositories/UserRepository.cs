using Dapper;
using MySql.Data.MySqlClient;
using Smort_api.Object.User;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    /// <summary>
    /// Repository for user-related database operations.
    /// Uses Dapper ORM for efficient, parameterized SQL queries.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        /// <summary>Database connection instance for all queries.</summary>
        private readonly IDbConnection _db;

        /// <summary>Constructor - initializes the repository with a database connection.</summary>
        public UserRepository(MySqlConnection connection)
        {
            _db = connection;
        }

        /// <summary>Checks if a user has already reported another user.</summary>
        public async Task<int> GetReportCountAsync(int reporterId, int reportedId)
        {
            const string sql = "SELECT COUNT(*) FROM Report_User WHERE User_Reporter_Id=@IdReporter AND User_Reported_Id=@IdReported;";
            return await _db.ExecuteScalarAsync<int>(sql, new { IdReporter = reporterId, IdReported = reportedId });
        }

        /// <summary>Records a user report in the database.</summary>
        public async Task ReportUserAsync(int reportedId, int reporterId, string reason)
        {
            const string sql = "INSERT INTO Report_User (User_Reported_Id, User_Reporter_Id, Reason, Reported_At) VALUES (@IdReported, @IdReporter, @Reason, @ReportedAt);";
            await _db.ExecuteAsync(sql, new { IdReported = reportedId, IdReporter = reporterId, Reason = reason, ReportedAt = System.DateTime.UtcNow });
        }

        /// <summary>Retrieves the current user's profile data (ID, picture, username).</summary>
        public async Task<GetMyUserDataSimpel?> GetMyProfileAsync(int id)
        {
            const string sql = "SELECT Id, Profile_Picture, Username FROM Users_Public WHERE Id=@Id;";
            return await _db.QueryFirstOrDefaultAsync<GetMyUserDataSimpel>(sql, new { Id = id });
        }

        /// <summary>Retrieves simplified user data for public profile viewing.</summary>
        public async Task<GetMyUserDataSimpel?> GetUserDataSimpleAsync(int id)
        {
            const string sql = "SELECT Id, Profile_Picture, Username FROM Users_Public WHERE Id=@Id;";
            return await _db.QueryFirstOrDefaultAsync<GetMyUserDataSimpel>(sql, new { Id = id });
        }

        /// <summary>Retrieves user profile data for display purposes.</summary>
        public async Task<IEnumerable<GetMyUserDataSimpel>> GetUserDataProfileAsync(int id)
        {
            const string sql = "SELECT Id, Profile_Picture, Username FROM Users_Public WHERE Id=@Id;";
            return await _db.QueryAsync<GetMyUserDataSimpel>(sql, new { Id = id });
        }
    }
}
