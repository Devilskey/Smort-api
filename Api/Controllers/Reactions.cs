using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object.User;
using System.Security.Claims;
using Tiktok_api.SignalRHubs;

namespace Tiktok_api.Controllers
{
    [ApiController]
    public class Reactions : ControllerBase
    {

        private readonly ILogger<Reactions> _logger;
        private readonly NotificationHubHandler _notificationHub;

        public Reactions(ILogger<Reactions> logger, NotificationHubHandler notificationHub)
        {
            _logger = logger;
            _notificationHub = notificationHub;
        }

        [HttpPost("Reactions/Like")]
        [Authorize]
        public async Task<IActionResult> Like(string contentId, string ContentType)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;


            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized();

            Console.WriteLine(ContentType);

            if (ContentType != "img" && ContentType != "vid")
            {
                return BadRequest();
            }

            string userId = User.FindFirstValue("Id");
            string username = User.FindFirstValue("Username");


            using (DatabaseHandler database = new DatabaseHandler())
            {

                string TypeOfLike = "";
                MySqlCommand HasAlreadyLiked = new MySqlCommand();

                HasAlreadyLiked.CommandText = "SELECT COUNT(Id) FROM Reaction WHERE User_Id=@user AND Content_Id=@content AND Reaction=@reaction AND Content_Type=@type; ";
                HasAlreadyLiked.Parameters.AddWithValue("@user", userId);
                HasAlreadyLiked.Parameters.AddWithValue("@content", contentId);
                HasAlreadyLiked.Parameters.AddWithValue("@reaction", "Like");
                HasAlreadyLiked.Parameters.AddWithValue("@type", ContentType);

                int Amount = database.GetNumber(HasAlreadyLiked);
                Console.WriteLine(Amount);
                MySqlCommand nextStep = new MySqlCommand();

                if (Amount == 0)
                {
                    TypeOfLike = "Like";
                    nextStep.CommandText = @"
                        INSERT INTO Reaction (User_Id, Content_Id, Content_Type, Reaction, Created_At) VALUES (@user, @content, @type, @reaction, @Created_At); 
                        SELECT Id, Username FROM Users_Public WHERE Id=(SELECT User_Id FROM Content WHERE id=@content);";
                }
                else
                {
                    TypeOfLike = "RemoveLike";
                    nextStep.CommandText = @"
                        DELETE FROM Reaction WHERE User_Id=@user AND Content_Id=@content AND Reaction=@reaction AND Content_Type=@type; ";
                }

                nextStep.Parameters.AddWithValue("@user", userId);
                nextStep.Parameters.AddWithValue("@content", contentId);
                nextStep.Parameters.AddWithValue("@reaction", "Like");
                nextStep.Parameters.AddWithValue("@type", ContentType);
                nextStep.Parameters.AddWithValue("@Created_At", new DateTime());

                string json = database.Select(nextStep);
                Console.WriteLine(json);

                if (TypeOfLike == "Like")
                {

                    UserData? user = JsonConvert.DeserializeObject<UserData[]>(json).First();

                    await _notificationHub.SendNotificationLikeToUser(user.Id.ToString(), $"{username} liked your video");
                }

                return Ok(TypeOfLike);
            }
        }
    }
}
