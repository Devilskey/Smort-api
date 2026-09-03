using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Smort_api.Handlers;
using Smort_api.Object.AskMe;
using Smort_api.Object.Database;
using System.Security.Claims;
using Dapper;

namespace Tiktok_api.Controllers.Content.AskMe
{
    [ApiController]
    public class AskeMeContent : ControllerBase
    {
        private ILogger<AskeMeContent> logger { get; set; }
        private readonly IDbConnection _db;

        public AskeMeContent(ILogger<AskeMeContent> _logger, IDbConnection db) {
            logger = _logger;
            _db = db;
        }

        [Authorize]
        [HttpPost]
        [Route("AskMe/CreateQuestion")]
        public async Task<IActionResult> CreateQuestion([FromBody] DTOCreateAskMe Question)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string id = User.FindFirstValue("app_user_id");

            var sqlCreateQuestion = @"
                INSERT INTO Content (User_Id, Type, Description, Created_At, Updated_At, Deleted_At) 
                VALUES (@Id,  @Type, @Description, @CreatedAt, @UpdatedAt, @DeletedAt); ";
            
            try
            {
                await _db.QueryAsync(sqlCreateQuestion, new
                {
                    Id=id, 
                    Description = Question.Content,
                    Type="Ask",
                    CreatedAt= DateTime.Now,
                    DeletedAt = DateTime.Now,
                    UpdatedAt=DateTime.Now,
                });
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
        public async Task<IActionResult> CreateAnswer([FromBody] DTOCreateAskMe Question, [FromRoute]int askId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string id = User.FindFirstValue("app_user_id");

            var sqlCreateQuestion = @"
                INSERT INTO Content_Answer (User_Id, Content_Id, Answer, Created_At, Updated_At) 
                VALUES (@Id, @AskMeId, @Answer, @CreatedAt, @UpdatedAt); ";
            
            try
            {
                await _db.QueryAsync(sqlCreateQuestion, new
                {
                    Id=id, 
                    Answer=Question.Content,
                    AskMeId=askId,
                    CreatedAt=DateTime.Now,
                    UpdatedAt=DateTime.Now
                });
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
        public async Task<IActionResult> getAnswer([FromRoute] int askId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string id = User.FindFirstValue("app_user_id");

            var sqlCreateQuestion = @"
                SELECT User_Id, Answer FROM Content_Answer WHERE Content_Id=@Id;";
            
            try
            {
                return Ok(await _db.QueryAsync<Object>(sqlCreateQuestion, new { Id = id }));
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
