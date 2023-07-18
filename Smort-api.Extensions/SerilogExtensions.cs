using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Smort_api.Extensions
{
    public static class SerilogExtensions
    {
        public static IServiceCollection AddSerilogLogging(this IServiceCollection service, IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Console()
                .CreateLogger();

            service.AddLogging(options => options.AddSerilog());
            return service;
        }
    }
}
