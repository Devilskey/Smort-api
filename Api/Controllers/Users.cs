using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Smort_api.Handlers;
using Smort_api.Object;

namespace Tiktok_api.Controllers
{
    [ApiController]
    public class Users : ControllerBase
    {
        private static ILogger Logger;

        public Users(ILogger<Users> logger) {
            Logger = logger;
        }

        [Route("users/CreateAccount")]
        [HttpPost]
        public Task<ActionResult> CreateAccount(UserData newUser)
        {
            if (newUser.Email == string.Empty || newUser.Password == string.Empty || newUser.Username == string.Empty)
                return Task.FromResult<ActionResult>(BadRequest());

            MySqlCommand addUser = new MySqlCommand();

            addUser.CommandText =
                @"INSERT INTO Users_private (Role_Id, Email, Password)
                 VALUES (1, @Email, @Password);
                 INSERT INTO Users_public (Person_Id, Username, Profile_Picture, Created_At, Updated_At, Deleted_At) 
                 VALUES (LAST_INSERT_ID(), @Username, @ProfilePicture, @CreatedAt, @UpdatedAt, @DeletedAt);";

            addUser.Parameters.AddWithValue("@Email", newUser.Email);
            addUser.Parameters.AddWithValue("@Password", newUser.Password);
            addUser.Parameters.AddWithValue("@Username", newUser.Username);
            addUser.Parameters.AddWithValue("@ProfilePicture", newUser.Profile_Picture);
            addUser.Parameters.AddWithValue("@DeletedAt", DateTime.Now);
            addUser.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
            addUser.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            using (DatabaseHandler databaseHandler = new DatabaseHandler()) {

                databaseHandler.EditDatabase(addUser);
            }

            Logger.Log(LogLevel.Information, $"Created User: {newUser.Username}");

            return Task.FromResult<ActionResult>(Ok());
        }

    }
}
