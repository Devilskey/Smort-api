using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Smort_api.Handlers;
using Smort_api.Object.AskMe;
using Smort_api.Object.Database;
using System.Security.Claims;

namespace Tiktok_api.Controllers.Content.AskMe
{
    [ApiController]
    public class AskeMeContent : ControllerBase
    {
        private ILogger<AskeMeContent> logger { get; set; }
        public AskeMeContent(ILogger<AskeMeContent> _logger) {
            logger = _logger;
        }

        [Authorize]
        [HttpPost]
        [Route("AskMe/CreateQuestion")]
        public IActionResult CreateQuestion([FromBody] DTOCreateAskMe Question)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string id = User.FindFirstValue("Id");

            MySqlCommand CreateQuestion = new MySqlCommand();
            CreateQuestion.CommandText = @"
                INSERT INTO Content (User_Id, Type, Description, Created_At, Updated_At, Deleted_At) 
                VALUES (@Id,  @Type, @Description, @CreatedAt, @UpdatedAt, @DeletedAt); ";

            CreateQuestion.Parameters.AddWithValue("@Id", id);
            CreateQuestion.Parameters.AddWithValue("@Description", Question.Content);
            CreateQuestion.Parameters.AddWithValue("@Type", "Ask");
            CreateQuestion.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            CreateQuestion.Parameters.AddWithValue("@DeletedAt", DateTime.Now);
            CreateQuestion.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

            try
            {
                using (DatabaseHandler database = new DatabaseHandler())
                {
                    database.EditDatabase(CreateQuestion);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }

            return Ok();
        }

        [Authorize]
        [HttpPost]
        [Route("AskMe/CreateAnswer/{askId}")]
        public IActionResult CreateAnswer([FromBody] DTOCreateAskMe Question, [FromRoute]int askId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string id = User.FindFirstValue("Id");

            MySqlCommand CreateQuestion = new MySqlCommand();
            CreateQuestion.CommandText = @"
                INSERT INTO Content_Answer (User_Id, Content_Id, Answer, Created_At, Updated_At) 
                VALUES (@Id, @AskMeId, @Answer, @CreatedAt, @UpdatedAt); ";

            CreateQuestion.Parameters.AddWithValue("@Id", id);
            CreateQuestion.Parameters.AddWithValue("@Answer", Question.Content);
            CreateQuestion.Parameters.AddWithValue("@AskMeId", askId);
            CreateQuestion.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            CreateQuestion.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

            try
            {
                using (DatabaseHandler database = new DatabaseHandler())
                {
                    database.EditDatabase(CreateQuestion);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }

            return Ok();
        }


        [Authorize]
        [HttpGet]
        [Route("AskMe/Answer/{askId}")]
        public IActionResult getAnswer([FromRoute] int askId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string id = User.FindFirstValue("Id");

            MySqlCommand CreateQuestion = new MySqlCommand();
            CreateQuestion.CommandText = @"
                SELECT User_Id, Answer FROM Content_Answer WHERE Content_Id=@Id;";

            CreateQuestion.Parameters.AddWithValue("@Id", askId);

            try
            {
                using (DatabaseHandler database = new DatabaseHandler())
                {
                   return Ok(database.Select(CreateQuestion));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("AskMe/Delete")]
        public void Post([FromBody] string value)
        {
        }
    }
}
