using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object;


namespace Tiktok_api.Controllers
{
    [ApiController]
    public class Users : ControllerBase
    {
        private readonly ILogger Logger;

        public Users(ILogger<Users> logger) {
            Logger = logger;
        }

        [Route("users/CreateAccount")]
        [HttpPost]
        public Task<ActionResult> CreateAccount(CreateAccount newUser)
        {
            // Checks if data is not empty
            if (string.IsNullOrEmpty(newUser.Email)  || string.IsNullOrEmpty(newUser.Password) || string.IsNullOrEmpty(newUser.Username))
                return Task.FromResult<ActionResult>(BadRequest());

            // Makes the using for the databasehandler that will be disposed when the task is done.
            using DatabaseHandler databaseHandler = new DatabaseHandler();

            // Checks if the email adress is already used in the database
            using MySqlCommand CheckIfExist = new MySqlCommand();
            CheckIfExist.CommandText = "SELECT Email FROM Users_Private WHERE Email = @Email";
            CheckIfExist.Parameters.AddWithValue("@Email", newUser.Email);

            string data = databaseHandler.Select(CheckIfExist);

            LoginObject[] Emails = JsonConvert.DeserializeObject<LoginObject[]>(data);

            // if the email adress already has been used return a bad request
            if (Emails != null && Emails.Length != 0) 
                return Task.FromResult<ActionResult>(BadRequest());

            using MySqlCommand GetUserNameAmount = new MySqlCommand();
            GetUserNameAmount.CommandText = "SELECT COUNT(*) FROM Users_Public WHERE Username LIKE @Username";
            GetUserNameAmount.Parameters.AddWithValue("@Username", $"{newUser.Username}#%");

            int IdNumber = databaseHandler.Count(GetUserNameAmount) + 1;

            Logger.Log(LogLevel.Information, $"{newUser.Username}#{IdNumber.ToString("D4")}");

            // Creates the new user and adds the data to the database
            using MySqlCommand addUser = new MySqlCommand();

            string[] Results = EncryptionHandler.HashAndSaltData(newUser.Password);

            addUser.CommandText =
                @"INSERT INTO Users_Private (Role_Id, Email, Password, Salt)
                 VALUES (1, @Email, @Password, @Salt);
                 INSERT INTO Users_Public (Person_Id, Username, Profile_Picture, Created_At, Updated_At, Deleted_At) 
                 VALUES (LAST_INSERT_ID(), @Username, @ProfilePicture, @CreatedAt, @UpdatedAt, @DeletedAt);";

            addUser.Parameters.AddWithValue("@Email", newUser.Email);
            addUser.Parameters.AddWithValue("@Password", Results[1]);
            addUser.Parameters.AddWithValue("@Salt", Results[0]);
            addUser.Parameters.AddWithValue("@Username", $"{newUser.Username}#{IdNumber.ToString("D4")}");
            addUser.Parameters.AddWithValue("@ProfilePicture", newUser.Profile_Picture);
            addUser.Parameters.AddWithValue("@DeletedAt", DateTime.Now);
            addUser.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
            addUser.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            databaseHandler.EditDatabase(addUser);

            // Logs the data 
            Logger.Log(LogLevel.Information, $"Created User: {newUser.Username}");

            return Task.FromResult<ActionResult>(Ok());
        }

        [Route("users/Login")]
        [HttpPost]
        public Task<string> Login(LoginObject User)
        {
            // Checks if the data that has been received is not empty
            if (string.IsNullOrEmpty(User.Email) || string.IsNullOrEmpty(User.Password))
                return Task.FromResult<string>("Data received Empty");

            // Checks if the email adress exists
            using MySqlCommand addUser = new MySqlCommand();
            addUser.CommandText =
                @"SELECT Password, Salt FROM Users_Private WHERE Email = @EmailGiven";

            addUser.Parameters.AddWithValue("@EmailGiven", User.Email);

            string jsonData = "[{}]";

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                jsonData = databaseHandler.Select(addUser);
                Logger.Log(LogLevel.Information, $"data: {jsonData}");
            }

            PasswordObject[]? Passwords = JsonConvert.DeserializeObject<PasswordObject[]>(jsonData);

            if (Passwords == null || Passwords.Length == 0)
                return Task.FromResult<string>("No accounts found");

            // Hashes the the password that has been givens
            string[] Results = EncryptionHandler.HashAndSaltData(User.Password);


            if (EncryptionHandler.VerifyData(Passwords[0], User.Password))
                return Task.FromResult<string>("I am a token");

            Logger.Log(LogLevel.Warning, $"Failed Login attempt at {DateTime.Now}");
            return Task.FromResult<string>("Failed to login");
        }

    }
}
