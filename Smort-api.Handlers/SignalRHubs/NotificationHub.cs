using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;


namespace Tiktok_api.SignalRHubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        public NotificationHub(ILogger<NotificationHub> logger) { _logger = logger; }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.UserIdentifier;

            await base.OnConnectedAsync();
        }
    }

    public class NotificationHubHandler
    {
        private readonly IHubContext<NotificationHub> _notifyHub;
        private readonly ILogger _logger;

        public NotificationHubHandler(IHubContext<NotificationHub> notifyHub, ILogger<NotificationHubHandler> logger)
        {
            _logger = logger;
            _notifyHub = notifyHub;
        }

        public async Task SendNotificationVideoToUser(string UserId, string messages)
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                _logger.Log(LogLevel.Information, messages);
                await _notifyHub.Clients.User(UserId).SendAsync("ReceiveNotificationVideo", messages);
            }
        }

        public async Task SendNotificationFollowToUser(string UserId, string messages)
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                _logger.Log(LogLevel.Information, messages);

                await _notifyHub.Clients.User(UserId).SendAsync("ReceiveNotificationFollow", messages);
            }
        }

        public async Task SendNotificationLikeToUser(string UserId, string messages)
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                _logger.Log(LogLevel.Information, messages);

                await _notifyHub.Clients.User(UserId).SendAsync("ReceiveNotificationLike", messages);
            }
        }

        public async Task SendNotificationFollowingToUser(string UserId, string messages)
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                _logger.Log(LogLevel.Information, messages);

                await _notifyHub.Clients.User(UserId).SendAsync("ReceiveNotificationFollowing", messages);
            }
        }
    }
}
