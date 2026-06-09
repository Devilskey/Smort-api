using Microsoft.Extensions.DependencyInjection;
using Smort_api.Handlers;
using Smort_api.Handlers.Repositories;
using Tiktok_api.Auth;
using Tiktok_api.BackgroundServices;
using Tiktok_api.Services;
using Tiktok_api.SignalRHubs;

namespace Smort_api.Extensions
{
    /// <summary>
    /// Extension methods for registering repositories and services in the dependency injection container.
    /// This centralizes all data access and business logic layer registrations.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Registers all repositories for data access layer.
        /// Repositories handle direct database operations using Dapper ORM.
        /// </summary>
        /// <param name="services">The service collection to register repositories into</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            // User repository - handles user profile, reports, and related queries
            services.AddScoped<IUserRepository, UserRepository>();

            // Image repository - handles image file paths and profile pictures
            services.AddScoped<IImageRepository, ImageRepository>();

            // Reactions repository - handles likes and other reactions
            services.AddScoped<IReactionsRepository, ReactionsRepository>();

            // Analytics repository - handles page views and tracking
            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

            // Content repository - handles video/image content queries
            services.AddScoped<IContentRepository, ContentRepository>();

            return services;
        }

        /// <summary>
        /// Registers all business logic services.
        /// Services orchestrate business rules and call repositories for data operations.
        /// </summary>
        /// <param name="services">The service collection to register services into</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // User service - orchestrates user-related operations
            services.AddScoped<IUserService, UserService>();

            // Firebase authentication service - validates Firebase tokens and creates local identity records
            services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();

            // Image service - handles image retrieval logic
            services.AddScoped<IImageService, ImageService>();

            // Reactions service - handles like/reaction logic
            services.AddScoped<IReactionsService, ReactionsService>();

            // Analytics service - handles analytics operations
            services.AddScoped<IAnalyticsService, AnalyticsService>();

            // Content service - handles content operations
            services.AddScoped<IContentService, ContentService>();

            return services;
        }

        /// <summary>
        /// Registers all singleton services that manage application-wide state.
        /// These are expensive objects created once and shared across the application.
        /// </summary>
        /// <param name="services">The service collection to register services into</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddSingletonServices(this IServiceCollection services)
        {
            // Process video service - background worker for video transcoding
            services.AddSingleton<ProcessVideoServices>();

            // Notification hub handler - manages SignalR connections for real-time notifications
            services.AddSingleton<NotificationHubHandler>();

            // Mail handler - sends email notifications
            services.AddSingleton<MailHandler>(new MailHandler());

            return services;
        }

        /// <summary>
        /// Registers all hosted services that run in the background.
        /// These run continuously or on schedules while the application is running.
        /// </summary>
        /// <param name="services">The service collection to register hosted services into</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            // Token cleanup service - periodically removes expired JWT tokens from blacklist
            services.AddHostedService<RemoveExpiredTokensServices>();

            // TODO: Enable video processing when ProcessVideoServices is stable
            // services.AddHostedService(provider => provider.GetRequiredService<ProcessVideoServices>());

            return services;
        }
    }
}
