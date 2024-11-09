
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Smort_api.Extensions
{
    public static class KestrelOptionExtension
    {
        public static IServiceCollection AddKestrelOptions(this IServiceCollection service)
        {
            service.Configure<KestrelServerOptions>(option =>
            {
                option.Limits.MaxRequestBodySize = null;
            });
            return service;
        }
    }
}
