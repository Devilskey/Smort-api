using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Tiktok_api.Controllers.Analytics
{
    [ApiController]
    public class WebsitePages : ControllerBase
    {
        private ILogger<WebsitePages> Logger;
        public WebsitePages(ILogger<WebsitePages> logger) {
            Logger = logger;
        }

        [Authorize]
        [Route("Analytics/WebPages/AddView")]
        [HttpPost]
        public Task<string> AddViewToPage()
        {
            //Check if page exists in pages
            //Add 1 view to the monthly Views table for that page 
            //If the page does not exits add the page to the table with 1 view
            return Task.FromResult("");
        }
    }
}