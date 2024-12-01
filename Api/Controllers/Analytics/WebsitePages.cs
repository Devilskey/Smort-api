using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Smort_api.Handlers;

namespace Tiktok_api.Controllers.Analytics
{
    [ApiController]
    public class WebsitePages : ControllerBase
    {
        private ILogger<WebsitePages> Logger;
        public WebsitePages(ILogger<WebsitePages> logger) {
            Logger = logger;
        }

        [Route("Analytics/WebPages/AddView")]
        [HttpPost]
        public Task<string> AddViewToPage(string Page)
        {
            MySqlCommand doesPageExist = new MySqlCommand();
            doesPageExist.CommandText = @"SELECT * FROM Pages WHERE Name=@PageName;";
            doesPageExist.Parameters.AddWithValue("@PageName", Page);

            using (DatabaseHandler handler = new DatabaseHandler())
            {
                var doesPageExistResult = handler.Select(doesPageExist);

                MySqlCommand addOneToMonthlyView = new MySqlCommand();
                addOneToMonthlyView.CommandText = @"UPDATE Page_Views_Monthly SET ViewCount = ViewCount + 1 WHERE Page_Id=@PageId";
                addOneToMonthlyView.Parameters.AddWithValue("@PageId", 1);
                handler.EditDatabase(addOneToMonthlyView);
            }

            //Check if page exists in pages
            //Add 1 view to the monthly Views table for that page 
            //If the page does not exits add the page to the table with 1 view
            return Task.FromResult("");
        }
        [Authorize]
        [Route("Analytics/WebPages/AddPage")]
        [HttpPost]
        public Task<string> AddPage(string Page)
        {


            //Check if page exists in pages
            //Add 1 view to the monthly Views table for that page 
            //If the page does not exits add the page to the table with 1 view
            return Task.FromResult("");
        }
    }
}