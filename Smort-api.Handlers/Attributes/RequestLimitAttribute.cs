using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Handlers.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequestLimitAttribute : ActionFilterAttribute
    {
        private readonly string _name;

        public RequestLimitAttribute(string name)
        {
            _name = name;
        }

        public int NumberOfRequest { get; set; } = 20;
        public int Seconds { get; set; } = 1;

        private static MemoryCache Cache { get; } = new MemoryCache(new MemoryCacheOptions());

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Sets up the Memery cache key / gets the callers ip
            var ipAddress = context.HttpContext.Request.HttpContext.Connection.RemoteIpAddress;
            var memoryCacheKey = $"{_name}-{ipAddress}";

            //Tries to get the value out if the cache 
            Cache.TryGetValue(memoryCacheKey, out int prevReqCount);

            //If the request of the user ois more then the max request returns the fullowing context result
            if (prevReqCount >= NumberOfRequest)
            {
                context.Result = new ContentResult
                {
                    Content = $"Request limit is exceeded. Try again in {Seconds} seconds."
                };
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            }
            else
            {
                //Sets exporation to time to the seconds and adds it to the cache
                var cacheEntryOptions =
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(Seconds));
                Cache.Set(memoryCacheKey, prevReqCount + 1, cacheEntryOptions);
            }
        }
    }
}
