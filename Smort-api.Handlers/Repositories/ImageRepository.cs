using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using System.Threading.Tasks;

namespace Smort_api.Handlers.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly IDbConnection _db;

        public ImageRepository(MySqlConnection connection)
        {
            _db = connection;
        }

        public async Task<int?> GetProfilePictureFileIdAsync(int userId)
        {
            const string sql = "SELECT Profile_Picture FROM Users_Public WHERE Id=@Id;";
            return await _db.ExecuteScalarAsync<int?>(sql, new { Id = userId });
        }

        public async Task<string?> GetFilePathByIdAsync(int fileId, bool isContent)
        {
            const string sqlContent = "SELECT File_Location FROM File_Content WHERE Id=@Id;";
            const string sqlImage = "SELECT File_Location FROM File_Image WHERE Id=@Id;";

            var sql = isContent ? sqlContent : sqlImage;
            return await _db.ExecuteScalarAsync<string?>(sql, new { Id = fileId });
        }
    }
}
