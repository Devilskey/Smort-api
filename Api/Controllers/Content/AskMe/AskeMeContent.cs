using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smort_api.Handlers;
using Smort_api.Object.AskMe;
using System.Security.Claims;
using Tiktok_api.Services;

namespace Tiktok_api.Controllers.Content.AskMe
{
    [ApiController]
    public class AskeMeContent : ControllerBase
    {
        private readonly IAskMeService _askMeService;

        public AskeMeContent(IAskMeService askMeService)
        {
            _askMeService = askMeService;
        }

        [Authorize]
        [HttpPost]
        [Route("AskMe/CreateQuestion")]
        public async Task<ActionResult> CreateQuestion([FromBody] CreateAskMe question)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string userId = User.FindFirstValue("app_user_id");

            try
            {
                await _askMeService.CreateQuestionAsync(userId, question.Content);
                return Ok();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("AskMe/CreateAnswer/{askId}")]
        public async Task<IActionResult> CreateAnswer([FromBody] CreateAskMe question, [FromRoute] int askId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            string userId = User.FindFirstValue("app_user_id");

            try
            {
                await _askMeService.CreateAnswerAsync(userId, askId, question.Content);
                return Ok();
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("AskMe/Answer/{askId}")]
        public async Task<IActionResult> GetAnswer([FromRoute] int askId)
        {
            string token = HttpContext.Request.Headers["Authorization"]!;

            if (JWTTokenHandler.IsBlacklisted(token))
                return Unauthorized("token is blacklisted");

            try
            {
                var answers = await _askMeService.GetAnswersAsync(askId);
                return Ok(answers);
            }
            catch (ArgumentException)
            {
                return BadRequest();
            }
            catch (Exception)
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
