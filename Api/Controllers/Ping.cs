using Microsoft.AspNetCore.Mvc;
using Smort_api.Handlers.Attributes;
using Smort_api.Handlers.Database;
using Smort_api.Object.Database.Interfaces;
using Smort_api.Object.OrmObjecten;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Tiktok_api.Controllers
{
    [ApiController]
    public class Ping : ControllerBase
    {
        [HttpGet("ping")]
        [RequestLimitAttribute("Pings")]
        public IActionResult Pings ()
        {
            return Ok();
        }

    }
}   