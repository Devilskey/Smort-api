using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object.Security;

namespace Tiktok_api.BackgroundServices
{
    public class RemoveExpiredTokensServices : BackgroundService
    {
        private readonly ILogger<RemoveExpiredTokensServices> _logger;

        public RemoveExpiredTokensServices(ILogger<RemoveExpiredTokensServices> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Log(LogLevel.Information, "Started Token Blacklist management services");

            JWTTokenHandler.BlackList = JWTTokenHandler.ReadBlackList();

            await ManageBlackList(stoppingToken);
        }

        private async Task ManageBlackList(CancellationToken stoppingToken)
        {
            List<JWTtokenBlacklistItem> blacklistItemsToRemove = new List<JWTtokenBlacklistItem>();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (JWTTokenHandler.BlackList != null)
                {
                    foreach (JWTtokenBlacklistItem blacklistItem in JWTTokenHandler.BlackList!)
                    {
                        if (blacklistItem.ExpireTime < DateTime.Now)
                        {
                            blacklistItemsToRemove.Add(blacklistItem);
                        }
                    }
                    foreach (JWTtokenBlacklistItem blacklistItem in blacklistItemsToRemove)
                    {
                        JWTTokenHandler.BlackList.Remove(blacklistItem);
                    }
                }
                JWTTokenHandler.WriteBlackList();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.Log(LogLevel.Information, "stopped Token Blacklist management services");
            JWTTokenHandler.WriteBlackList();
            await base.StopAsync(stoppingToken);
        }
    }
}
