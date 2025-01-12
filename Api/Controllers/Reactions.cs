using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Smort_api.Handlers;
using System.Security.Claims;

namespace Tiktok_api.Controllers
{
    [ApiController]
    public class Reactions : ControllerBase
    {

        //[HttpGet]
        //[Route("Reactions/AmountOfLikes")]
        //public IActionResult AmountOfLikes(string videoId)
        //{

        //    using (DatabaseHandler database = new DatabaseHandler())
        //    {
        //        MySqlCommand HasAlreadyLiked = new MySqlCommand();

        //        HasAlreadyLiked.CommandText = "SELECT COUNT(Id) FROM Reaction WHERE Video_Id=@video AND Reaction=@reaction; ";
        //        HasAlreadyLiked.Parameters.AddWithValue("@video", videoId);
        //        HasAlreadyLiked.Parameters.AddWithValue("@reaction", "Like");

        //        int Amount = database.GetNumber(HasAlreadyLiked);

        //        return Ok(Amount);
        //    }
        //}

        [HttpPost]
        [Authorize]
        [Route("Reactions/Like")]
        public IActionResult Like(string contentId, string ContentType)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized();

            Console.WriteLine(ContentType);

            if(ContentType != "img" && ContentType != "vid")
            {
                return BadRequest();
            }

            string userId = User.FindFirstValue("Id");

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
                MySqlCommand nextStep = new MySqlCommand();

                if (Amount == 0)
                {
                    TypeOfLike = "Like";
                    nextStep.CommandText = "INSERT INTO Reaction (User_Id, Content_Id, Content_Type, Reaction) VALUES (@user, @content, @type, @reaction); ";
                }
                else
                {
                    TypeOfLike = "RemoveLike";
                    nextStep.CommandText = "DELETE FROM Reaction WHERE User_Id=@user AND Content_Id=@content AND Reaction=@reaction AND Content_Type=@type; ";
                }
                nextStep.Parameters.AddWithValue("@user", userId);
                nextStep.Parameters.AddWithValue("@content", contentId);
                nextStep.Parameters.AddWithValue("@reaction", "Like");
                nextStep.Parameters.AddWithValue("@type", ContentType);

                Console.WriteLine( database.Select(nextStep));
                return Ok(TypeOfLike);
            }
        }
    }
}
