using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Smort_api.Object.DTO;
using Tiktok_api.Services;

namespace Tiktok_api.Controllers.Content;

[Authorize]
[ApiController]
public class Search
    (ISearchService _searchService) : ControllerBase
{
    [Route("search/all")]
    [HttpGet]
    public async Task<ActionResult<SearchAllDto>> SearchAll(string query)
    {
        var userId = User.FindFirstValue("app_user_id");
        if (Int32.TryParse(userId, out var userIdInt))
        {
            var data = await _searchService.SearchAllAsync(query, userIdInt);
            return Ok(data);
        }

        return Forbid();
    }
}