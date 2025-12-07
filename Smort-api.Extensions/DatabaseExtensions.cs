using FluentMigrator.Runner;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smort_api.Handlers.Database;

namespace Smort_api.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection MigrateDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetSection("Database:ConnectionString").Get<string>()! ;

            if(connectionString == null)
            {
                Console.WriteLine("No Connectionstring");
                return services;
            }

            services
                .AddFluentMigratorCore()
                .ConfigureRunner(r =>
                r.AddMySql8()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(PrimairyDatabaseMigration).Assembly).For.Migrations())
                .AddLogging(l => l.AddFluentMigratorConsole());


            return services;

        }

    }
}
