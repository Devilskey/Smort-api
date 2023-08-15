using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Smort_api.Handlers
{
    public class DatabaseHandler : IDisposable
    {
        private string? Username { get; set; }
        private string? Password { get; set; }
        private string? Server  { get; set; }
        private string? DatabaseName { get; set; }

        private readonly ILogger Logger;

        private MySqlConnection connection;

        public DatabaseHandler(ILogger<DatabaseHandler> logger = null) {
            Logger = logger;
            Initialize();
        }

        /// <summary>
        /// Gets the env variables and makes a connection.
        /// </summary>
        void Initialize()
        {
            Username = Environment.GetEnvironmentVariable("UsernameDb") ?? "root";
            Password = Environment.GetEnvironmentVariable("PasswordDb") ?? "password";
            Server = Environment.GetEnvironmentVariable("ServerDb") ?? "10.0.0.12";
            DatabaseName = Environment.GetEnvironmentVariable("DatabaseName") ?? "SmortTestDb";

            if (Username == "" || Password == "" || Server == "" || DatabaseName == "")
                System.Console.WriteLine("No Env found");

            string connectionString = $"server={Server};port=3306;uid={Username};pwd={Password};database={DatabaseName};";
            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

        /// <summary>
        /// Returns the selected data in a json string
        /// </summary>
        /// <param name="SqlCommand"></param>
        /// <returns></returns>
        public string Select(MySqlCommand SqlCommand)
        {
            SqlCommand.Connection = connection;
            try
            {
                using (MySqlDataReader Reader = SqlCommand.ExecuteReader())
                {
                    return SqlReaderToJson(Reader);
                }
            }
            catch (Exception ex)
            {
                return $"Json Error {ex}";
            }

        }

        public int GetNumber(MySqlCommand SqlCommand)
        {
            SqlCommand.Connection = connection;
            try
            {
                using (MySqlDataReader Reader = SqlCommand.ExecuteReader())
                {
                    return sqlReaderToInt(Reader);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"message: {ex.Message}, source {ex.Source}");
                return 0;
            }
        }

        /// <summary>
        /// Use this funciton For Update Delete and Post
        /// </summary>
        /// <param name="SqlCommand"></param>
        public void EditDatabase(MySqlCommand SqlCommand)
        {
            SqlCommand.Connection = connection;
            try
            {
                SqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Gets the data from a select and returns it into a json format
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public string SqlReaderToJson(MySqlDataReader reader)
        {
            List<object> objects = new();
            while (reader.Read())
            {
                IDictionary<string, object> record = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    record.Add(reader.GetName(i), reader[i]);
                }
                objects.Add(record);
            }
            return JsonConvert.SerializeObject(objects);
        }

        /// <summary>
        /// Gets the sql data returns an int
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public int sqlReaderToInt(MySqlDataReader reader)
        {
            if (reader.Read())
            {
                return reader.GetInt32(0);
            }
            
            return 0;
        }

        /// <summary>
        /// Migrates the database
        /// </summary>
        /// <param name="sqlFileContent"></param>
        public void Migrate(string[] sqlFileContent)
        {
            MySqlCommand sqlCommand = new MySqlCommand();
            foreach (string query in sqlFileContent) {
                sqlCommand.CommandText = query;
                sqlCommand.Connection = connection;
                try
                {
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Disposes the data
        /// </summary>
        public void Dispose()
        {
            Username = string.Empty;
            Password = string.Empty;
            Server = string.Empty;
            DatabaseName = string.Empty;
            connection.Close();
            connection.Dispose();
        }
    }
}