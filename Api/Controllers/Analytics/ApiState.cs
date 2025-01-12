using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tiktok_api.Controllers.Analytics
{
    [ApiController]
    public class ApiState : ControllerBase
    {
        private ILogger<ApiState> logger { get; set; }
        private IConfiguration _config { get; set; }

        public ApiState(ILogger<ApiState> logger, IConfiguration config)
        {
            this.logger = logger;
            _config = config;
        }

        [HttpGet]
        [Route("Api/Status")]
        public Task<string> Get() 
        {
            logger.LogInformation($"{DateTime.Now} -- Api State Alive");
            return Task.FromResult("Alive");
        }
    }
}
