using Microsoft.AspNetCore.Mvc;
using Smort_api.Handlers.Attributes;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tiktok_api.Controllers
{
    [ApiController]
    public class Ping : ControllerBase
    {
        [HttpGet]
        [Route("")]
        [RequestLimitAttribute("Pings")]
        public IActionResult Pings ()
        {
            return Ok();
        }

    }
}
