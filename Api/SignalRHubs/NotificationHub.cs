using Microsoft.AspNetCore.SignalR;
using Tiktok_api.Controllers.Users;
using System.Security.Claims;
using System.Collections.Concurrent;
using MySqlX.XDevAPI;
using Microsoft.AspNetCore.Authorization;

namespace Tiktok_api.SignalRHubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        public NotificationHub(ILogger<NotificationHub> logger) { _logger = logger; }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.UserIdentifier;

            if (!string.IsNullOrEmpty(userId))
            {
                await Clients.User(userId).SendAsync("ReceiveNotification", "Welcome back!");
            }

            await base.OnConnectedAsync();
        }
    }

    public class NotificationHubHandler
    {
        private readonly IHubContext<NotificationHub> _notifyHub;
        public NotificationHubHandler(IHubContext<NotificationHub> notifyHub)
        {
            _notifyHub = notifyHub;
        }

        public async Task SendNotificationToUser(string UserId, string messages)
        {
            if (!string.IsNullOrEmpty(UserId))
            {
                await _notifyHub.Clients.User(UserId).SendAsync("ReceiveNotification", messages);
            }
        }
    }
}
