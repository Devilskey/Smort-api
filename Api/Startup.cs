using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Smort_api.Extensions;
using System.IO;
using System.Text;
using Tiktok_api.BackgroundServices;
<<<<<<< Updated upstream
=======
using Tiktok_api.Controllers;
using Tiktok_api.SignalRHubs;
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
                options.AddPolicy("anyCors", Policy =>
                    Policy.AllowAnyOrigin()
=======
                options.AddPolicy("SmortSecureOnly", Policy =>
                    Policy.WithOrigins("https://smorthub.nl", "https://smorthub.nl/", "http://localhost:3000", "https://localhost:3000")
                        .AllowCredentials()
>>>>>>> Stashed changes
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
                        Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SecretTokenJWT") ?? Configuration["JwtSettings:Key"]!)),
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
<<<<<<< Updated upstream
            app.UseCors("anyCors");
=======

            app.UseMiddleware<LogRequestMiddleware>();

            app.UseCors("SmortSecureOnly");
>>>>>>> Stashed changes

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
