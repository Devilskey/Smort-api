using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Smort_api.Object.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Handlers.Database
{
    public class DatabaseORMHandler
    {
        private string? Username { get; set; }
        private string? Password { get; set; }
        private string? Server { get; set; }
        private string? DatabaseName { get; set; }

        private readonly ILogger Logger;

        private MySqlConnection connection;

        public DatabaseORMHandler(ILogger<DatabaseORMHandler> logger = null)
        {
            Logger = logger;

            Username = Environment.GetEnvironmentVariable("UsernameDb") ?? "root";
            Password = Environment.GetEnvironmentVariable("PasswordDb") ?? "password";
            Server = Environment.GetEnvironmentVariable("ServerDb") ?? "localhost";
            DatabaseName = Environment.GetEnvironmentVariable("DatabaseName") ?? "SmortTestDb";

            if (Username == "" || Password == "" || Server == "" || DatabaseName == "")
                Console.WriteLine("No Env found");

            string connectionString = $"server={Server};port=3306;uid={Username};pwd={Password};database={DatabaseName};";
            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

    
    }
}
