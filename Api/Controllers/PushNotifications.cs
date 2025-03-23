using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Smort_api.Handlers;

namespace Tiktok_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PushNotifications : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly VapidKeyGenerator keys;
        public PushNotifications()
        {
            keys = new VapidKeyGenerator().GenerateVapidKeys();
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe([FromBody] PushSubscription subscription)
        {

            return Ok();
        }

    }

    public class PushSubscription
    {
        public string pushEndpoint { get; set; }
        public string p256dh { get; set; }
        public string auth { get; set; }
        public string subject { get; set; }
        public string publicKey { get; set; }
        public string privateKey { get; set; }
        public dynamic payload { get; set; }
    }
}
