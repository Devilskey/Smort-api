using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Tiktok_api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddMvc();
            services.AddSwaggerGen();
            services.AddEndpointsApiExplorer();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("./Logs/log.json")
                .CreateLogger();

            services.AddLogging(c => c.AddSerilog());

        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
          

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
