using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Smort_api.Extensions;
using System.IO;
using System.Text;
using Tiktok_api.BackgroundServices;

namespace Tiktok_api
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy("anyCors", Policy =>
                    Policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });


            services.AddAuthentication(config =>
            {
                config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                config.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(config =>
            {
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = Configuration["JwtSettings:Issuer"],
                    ValidAudience = Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Configuration["JwtSettings:Key"]!)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

            services.AddSwaggerSecurityConfiguration();

            services.AddAuthorization();

            services.AddMvc();

            services.AddHostedService<RemoveExpiredTokensServices>();

            services.AddEndpointsApiExplorer();

            services.AddSerilogLogging(Configuration);

            services.AddKestrelOptions();

        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors("anyCors");

            app.UseSwaggerDocumentation();

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.MigrateDatabase(Configuration);

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.LogApiInfo();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
