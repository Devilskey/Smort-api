using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tiktok_api.Services;

namespace Tiktok_api.Controllers.Analytics
{
    [ApiController]
    public class WebsitePages
        (ILogger<WebsitePages> logger, IAnalyticsService analyticsService) 
        : ControllerBase {
    
        [Route("Analytics/WebPages/AddView")]
        [HttpPost]
        public async Task<string> AddViewToPage(string Page)
        {
            await analyticsService.AddViewAsync(Page);
            return "";
        }
        [Authorize]
        [Route("Analytics/WebPages/AddPage")]
        [HttpPost]
        public async Task<int> AddPage(string Page)
        {
            return await analyticsService.AddPageAsync(Page);
        }
    }
}
