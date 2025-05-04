using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Smort_api.Handlers;

namespace Smort_api.Extensions
{
    public static class DatabaseExtensions
    {
        public static string SqlFile = "./Migrate.sql";

        public static void MigrateDatabase(this IApplicationBuilder app, IConfiguration configuration)
        {
            using (DatabaseHandler databaseHandler = new())
            {
                string[] sqlContent = File.ReadAllLines(SqlFile);

                databaseHandler.Migrate(sqlContent);
            }
        }
    }
}
