namespace Tiktok_api.Controllers
{
    public class LogRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogRequestMiddleware> _logger;

        public LogRequestMiddleware(RequestDelegate next, ILogger<LogRequestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var origin = context.Request.Headers["Origin"].ToString();
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            // Check if the origin is not allowed in the CORS policy
            var isAllowedOrigin = YourCorsPolicyCheckMethod(context);

            if (!isAllowedOrigin)
            {
                // Log the origin and IP address
                _logger.LogWarning($"Request from Origin: {origin}, IP Address: {ipAddress} is not allowed by CORS policy.");
            }

            await _next(context);
        }

        private bool YourCorsPolicyCheckMethod(HttpContext context)
        {
            // Implement the logic that checks if the origin is allowed in your CORS policy
            var origin = context.Request.Headers["Origin"].ToString();
            var allowedOrigins = new List<string> { "https://smorthub.nl", "https://smorthub.nl/", "http://localhost:3000", "https://localhost:3000" };

            return allowedOrigins.Contains(origin);
        }
    }

}
