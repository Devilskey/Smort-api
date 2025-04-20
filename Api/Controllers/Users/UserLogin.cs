using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object;
using Smort_api.Object.Database;
using Smort_api.Object.Security;
using Smort_api.Object.User;
using Tiktok_api.Settings_Api;

namespace Tiktok_api.Controllers.Users
{
    public partial class Users : ControllerBase
    {
        /// <summary>
        /// Create account For user
        /// </summary>
        /// <param name="newUser"></param>
        /// <returns></returns>
        [Route("users/CreateAccount")]
        [HttpPost]
        public Task<ActionResult> CreateAccount(CreateAccount newUser)
        {
            // Checks if data is not empty
            if (string.IsNullOrEmpty(newUser.Email) || string.IsNullOrEmpty(newUser.Password) || string.IsNullOrEmpty(newUser.Username))
                return Task.FromResult<ActionResult>(BadRequest("Not all the data is Filled in"));

            // Makes the using for the databasehandler that will be disposed when the task is done.
            using DatabaseHandler databaseHandler = new DatabaseHandler();

            // Checks if the email adress is already used in the database
            using MySqlCommand CheckIfExist = new MySqlCommand();
            CheckIfExist.CommandText = "SELECT Email FROM Users_Private WHERE Email = @Email;";
            CheckIfExist.Parameters.AddWithValue("@Email", newUser.Email);

            string data = databaseHandler.Select(CheckIfExist);

            CheckIfExist.Dispose();

            LoginObject[] Emails = JsonConvert.DeserializeObject<LoginObject[]>(data)!;

            // if the email adress already has been used return a bad request
            if (Emails != null && Emails.Length != 0)
                return Task.FromResult<ActionResult>(BadRequest("Already an account using this Email"));

            using MySqlCommand CheckUsernameExist = new MySqlCommand();

            CheckUsernameExist.CommandText = "SELECT COUNT(*) FROM Username_Counter WHERE Username=@Username;";
            CheckUsernameExist.Parameters.AddWithValue("@Username", $"{newUser.Username}");

            CheckUsernameExist.Dispose();

            int Exist = databaseHandler.GetNumber(CheckUsernameExist);
            int id = 0;

            if (Exist == 0)
            {
                //Insert New Username
                using MySqlCommand GetUserNameAmount = new MySqlCommand();

                GetUserNameAmount.CommandText = "INSERT INTO Username_Counter (Username, Amount, Created_At, Updated_At) VALUES (@Username, @Amount, @Created_At, @Update_At);";

                GetUserNameAmount.Parameters.AddWithValue("@Username", $"{newUser.Username}");
                GetUserNameAmount.Parameters.AddWithValue("@Amount", 0);
                GetUserNameAmount.Parameters.AddWithValue("@Created_At", DateTime.Now);
                GetUserNameAmount.Parameters.AddWithValue("@Deleted_At", DateTime.Now);
                GetUserNameAmount.Parameters.AddWithValue("@Update_At", DateTime.Now);

                databaseHandler.EditDatabase(GetUserNameAmount);
                GetUserNameAmount.Dispose();
            }
            else
            {
                using MySqlCommand GetUserNameAmount = new MySqlCommand();

                GetUserNameAmount.CommandText = "SELECT Amount FROM Username_Counter WHERE Username=@Username;";

                GetUserNameAmount.Parameters.AddWithValue("@Username", $"{newUser.Username}");

                id = databaseHandler.GetNumber(GetUserNameAmount);
                GetUserNameAmount.Dispose();
            }

            //Creates File
            MySqlCommand GetAndAddProfilePicture = new MySqlCommand();
            GetAndAddProfilePicture.CommandText =
                @"INSERT INTO File (File_Name, File_Location, Created_At) VALUES (@Name, @Location, @Created);
                  SELECT LAST_INSERT_ID();";

            var ImageGUID = Guid.NewGuid().ToString();


            GetAndAddProfilePicture.Parameters.AddWithValue("@Name", $"{ImageGUID}.webp");
            GetAndAddProfilePicture.Parameters.AddWithValue("@Location", $"./ProfilePictures/{ImageGUID}");
            GetAndAddProfilePicture.Parameters.AddWithValue("@Created", DateTime.Now);

            int FileId = databaseHandler.GetNumber(GetAndAddProfilePicture);

            foreach (var sizes in ContentSizingObjects.ProfilePictures)
            {

                float percentageLesser = ((float)sizes.Width / (float)newUser.size.Width);

                Console.WriteLine(percentageLesser);

                int newWidth = (int)(percentageLesser * newUser.size.Width);
                Console.WriteLine(newWidth);

                int newHeight = (int)(percentageLesser * newUser.size.Height);
                Console.WriteLine(newHeight);


                var ResizedFilePost = ImageHandler.ChangeSizeOfImage(newUser.ProfilePicture, newWidth, newHeight);

                ImageHandler.SaveProfilePictures(ResizedFilePost, $"{ImageGUID}_{sizes.Size}.webp");
            }

            // Creates the new user and adds the data to the database
            MySqlCommand addUser = new MySqlCommand();

            string[] Results = EncryptionHandler.HashAndSaltData(newUser.Password);

            addUser.CommandText =
                @"INSERT INTO Users_Private (Role_Id, Email, Password, Salt)
                 VALUES (1, @Email, @Password, @Salt);
                 INSERT INTO Users_Public (Person_Id, Username, Profile_Picture, Created_At, Updated_At, Deleted_At) 
                 VALUES (LAST_INSERT_ID(), @Username, @ProfilePicture, @CreatedAt, @UpdatedAt, @DeletedAt);";

            addUser.Parameters.AddWithValue("@Email", newUser.Email);
            addUser.Parameters.AddWithValue("@Password", Results[1]);
            addUser.Parameters.AddWithValue("@Salt", Results[0]);
            addUser.Parameters.AddWithValue("@Username", $"{newUser.Username}#{(id + 1).ToString("D4")}");
            addUser.Parameters.AddWithValue("@ProfilePicture", FileId.ToString());
            addUser.Parameters.AddWithValue("@DeletedAt", DateTime.Now);
            addUser.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
            addUser.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

            databaseHandler.EditDatabase(addUser);

            addUser.Dispose();

            using MySqlCommand UpdateUsernameAmount = new MySqlCommand();

            UpdateUsernameAmount.CommandText = "UPDATE Username_Counter SET Amount=@Amount, Updated_At=@UpdatedAt WHERE Username=@Username";

            UpdateUsernameAmount.Parameters.AddWithValue("@Username", $"{newUser.Username}");
            UpdateUsernameAmount.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

            UpdateUsernameAmount.Parameters.AddWithValue("@Amount", id + 1);


            databaseHandler.EditDatabase(UpdateUsernameAmount);
            UpdateUsernameAmount.Dispose();

            // Logs the data 
            Logger.Log(LogLevel.Information, $"Created User: {newUser.Username}");

            using (MailHandler mail = new MailHandler())
            {
                mail.SendMail(newUser.Email);
            }

            return Task.FromResult<ActionResult>(Ok("User Created"));
        }

        /// <summary>
        /// Login for a user returns a jwt token
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        [Route("users/Login")]
        [HttpPost]
        public Task<string> Login(LoginObject User)
        {
            // Checks if the data that has been received is not empty
            if (string.IsNullOrEmpty(User.Email) || string.IsNullOrEmpty(User.Password))
                return Task.FromResult("Data received Empty");

            // Checks if the email adress exists
            using MySqlCommand addUser = new MySqlCommand();
            addUser.CommandText =
                @"SELECT Password, Salt FROM Users_Private WHERE Email = @EmailGiven;";

            addUser.Parameters.AddWithValue("@EmailGiven", User.Email);

            string jsonData = "[{}]";

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                jsonData = databaseHandler.Select(addUser);
            }

            MySqlCommand getId = new MySqlCommand();
            getId.CommandText = "SELECT Id, Username FROM Users_Public WHERE Person_Id=(SELECT Id FROM Users_Private WHERE Email=@Email);";

            getId.Parameters.AddWithValue("@Email", User.Email);

            UserData[]? user = null;

            using (DatabaseHandler databaseHandler = new DatabaseHandler())
            {
                string json = databaseHandler.Select(getId);
                user = JsonConvert.DeserializeObject<UserData[]>(json);
            }

            if (user == null) return Task.FromResult($"User Id not found");

            PasswordObject[]? Passwords = JsonConvert.DeserializeObject<PasswordObject[]>(jsonData);


            if (Passwords == null || Passwords.Length == 0)
                return Task.FromResult("No accounts found");

            // Hashes the the password that has been givens
            string[] Results = EncryptionHandler.HashAndSaltData(User.Password);

            if (EncryptionHandler.VerifyData(Passwords[0], User.Password))
            {
                string token = JWTTokenHandler.GenerateToken(User, user.First().Id.ToString(), user.First().UserName);
                return Task.FromResult(token);
            }

            Logger.Log(LogLevel.Warning, $"Failed Login attempt at {DateTime.Now}");

            return Task.FromResult("Failed to login");
        }
    }
}
