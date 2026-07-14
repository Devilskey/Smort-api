using System;
using System.Linq;
using System.Text;
using Extensions;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Serilog;
using Smort_api.Extensions;
using Smort_api.Handlers;
using Tiktok_api.Auth;
using Tiktok_api.BackgroundServices;
using Tiktok_api.SignalRHubs;

namespace Tiktok_api
{
    /// <summary>
    /// Application entry point for the Smort API.
    /// Configures dependency injection, middleware, authentication, and database connections.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point. Initializes and runs the web application.
        /// </summary>
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // ========== CONFIGURATION SETUP ==========
                // Load settings from multiple JSON files and environment variables
                builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
                    config
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("serilog.json", optional: false, reloadOnChange: true)
                        .AddEnvironmentVariables();
                });

                var services = builder.Services;
                var configuration = builder.Configuration;

                services.AddFirebaseAuth(configuration);


                // ========== BASIC MVC & API SETUP ==========
                // Register controllers and API explorer for Swagger
                services.AddControllers();
                services.AddEndpointsApiExplorer();

                // ========== CORS CONFIGURATION ==========
                // Allow specific origins to access the API
                string[] allowedUrls = configuration
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

                // ========== AUTHENTICATION & JWT ==========
                // Configure JWT bearer token authentication for local Smort API tokens
                services.AddMemoryCache();
                services.AddTransient<IClaimsTransformation, FirebaseClaimsTransformer>();

                // Add authorization policies
                services.AddAuthorization();


                // ========== MVC & SIGNALR ==========
                services.AddMvc();
                services.AddSignalR();

                // ========== API DOCUMENTATION (SWAGGER) ==========
                services.AddSwaggerSecurityConfiguration();

                // ========== DATABASE SETUP ==========
                // Register database connection and migrations
                string? connectionString = configuration.GetSection("Database:ConnectionString").Get<string>();
                if (!string.IsNullOrEmpty(connectionString))
                {
                    services.AddTransient<MySqlConnection>(x => new MySqlConnection(connectionString));
                    services.MigrateDatabase(configuration);
                    Console.WriteLine("✓ Database connection configured");
                }
                else
                {
                    Console.WriteLine("⚠ WARNING: No database connection string found. Database features disabled.");
                }

                // ========== DEPENDENCY INJECTION - REPOSITORIES & SERVICES ==========
                // Use extension methods to register all data access and business logic layers
                services.AddRepositories();           // Register all repositories (data access)
                services.AddApplicationServices();    // Register all business logic services
                services.AddSingletonServices();      // Register singleton services (NotificationHub, etc.)
                services.AddBackgroundServices();     // Register hosted background services

                // ========== CONFIGURATION & LOGGING SERVICES ==========
                services.AddSerilogLogging(configuration);
                services.AddKestrelOptions();

                // ========== COMPRESSION ==========
                // Configure response compression for supported MIME types
                string[] allowedMimicTypes = configuration
                    .GetSection("AllowedMimicTypes")
                    .Get<string[]>() ?? Array.Empty<string>();

                services.AddResponseCompression(options =>
                {
                    options.EnableForHttps = true;
                    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(allowedMimicTypes);
                });


                // ========== LOGGING SETUP ==========
                // Configure Serilog for structured logging throughout the application
                builder.Host.UseSerilog();

                // ========== BUILD APPLICATION ==========
                var app = builder.Build();

                // ========== MIDDLEWARE PIPELINE ==========
                // Order matters: executed top to bottom for requests

                // Enable CORS
                app.UseCors("SmortSecureOnly");

                // API documentation and testing
                app.UseSwaggerDocumentation();

                // Logging middleware
                app.UseSerilogRequestLogging();

                // HTTPS redirect (only in production to avoid development issues)
                if (!app.Environment.IsDevelopment())
                {
                    app.UseHttpsRedirection();
                }

                // Compression
                app.UseResponseCompression();

                // Routing
                app.UseRouting();

                // Authentication & Authorization
                app.UseAuthentication();
                app.UseAuthorization();

                // Log API startup info
                app.LogApiInfo();

                // ========== DATABASE MIGRATIONS (Optional) ==========
                // Run database migrations at startup
                try
                {
                    using (var scope = app.Services.CreateScope())
                    {
                        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                        runner.MigrateUp();
                        Console.WriteLine("✓ Database migrations completed");
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Database migrations failed - app will continue without database");
                    Console.WriteLine($"⚠ Migration warning: {ex.Message}");
                }

                // ========== ENDPOINT MAPPING ==========
                // Map controller endpoints
                app.MapControllers();

                // Map SignalR hub for real-time notifications
                app.MapHub<NotificationHub>("/Notify");

                // ========== STARTUP INFO ==========
                Console.WriteLine("\n╔════════════════════════════════════════╗");
                Console.WriteLine("║      🚀 SMORT API STARTING 🚀         ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
                Console.WriteLine($"Database: {(!string.IsNullOrEmpty(connectionString) ? "✓ Configured" : "✗ Not configured")}");
                Console.WriteLine("════════════════════════════════════════\n");

                // ========== RUN APPLICATION ==========
                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ FATAL ERROR: {ex.Message}");
                Console.WriteLine($"Stack Trace:\n{ex.StackTrace}");
                Log.Fatal(ex, "Application terminated unexpectedly");
                Environment.Exit(1);
            }
        }
    }
}
