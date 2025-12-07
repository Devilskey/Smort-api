using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Serilog;
using Smort_api.Extensions;
using Smort_api.Handlers;
using System.Text;
using Tiktok_api.BackgroundServices;
using Tiktok_api.SignalRHubs;

namespace Tiktok_api
{
    public class Startup
    {
        private IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            string[] allowedUrls = _configuration
                .GetSection("CorsAllowedUrls")
                .Get<string[]>() ?? Array.Empty<string>();

            services.AddCors(options =>
            {
                options.AddPolicy("SmortSecureOnly", Policy =>
                    Policy.WithOrigins(allowedUrls)
                        .AllowCredentials()
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
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SecretTokenJWT") ?? _configuration["JwtSettings:Key"]!)),
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
            services.AddSingleton<MailHandler>(new MailHandler());


            string connectionString = _configuration.GetSection("Database:ConnectionString").Get<string>()!;

            if (connectionString == null)
            {
                Console.WriteLine("No connection string found");
                return;
            }
            else
            {
                services.AddTransient<MySqlConnection>(x => new MySqlConnection(connectionString));
            }

            services.MigrateDatabase(_configuration);


            services.AddHostedService<RemoveExpiredTokensServices>();

            services.AddEndpointsApiExplorer();

            services.AddSerilogLogging(_configuration);

            services.AddKestrelOptions();

            string[] allowedMimicTypes = _configuration
                .GetSection("AllowedMimicTypes")
                .Get<string[]>() ?? Array.Empty<string>();


            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(allowedMimicTypes);
            });

        }
        public void Configure(IApplicationBuilder app)
        {
            app.UseCors("SmortSecureOnly");

            app.UseSwaggerDocumentation();

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();


            app.UseResponseCompression();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.LogApiInfo();


            using (var scope = app.ApplicationServices.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/Notify");
            });

        }
    }
}
