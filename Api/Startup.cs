using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Smort_api.Extensions;
using System.IO;
using System.Text;
using Tiktok_api.BackgroundServices;
using Tiktok_api.SignalRHubs;

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
                options.AddPolicy("SmortSecureOnly", Policy =>
                    Policy.WithOrigins("https://devilskey.nl", "https://smorthub.nl", "http://localhost:3000", "https://localhost:3000").AllowCredentials()
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

                config.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/Notify"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };

            });

            services.AddSwaggerSecurityConfiguration();

            services.AddAuthorization();

            services.AddMvc();

            services.AddSignalR();

            services.AddHostedService(provider => provider.GetRequiredService<ProcessVideoServices>());

            services.AddSingleton<ProcessVideoServices>();
            services.AddSingleton<NotificationHubHandler>();

            services.AddHostedService<RemoveExpiredTokensServices>();

            services.AddEndpointsApiExplorer();

            services.AddSerilogLogging(Configuration);

            services.AddKestrelOptions();

        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors("SmortSecureOnly");

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
                endpoints.MapHub<NotificationHub>("/Notify");
            });

        }
    }
}
