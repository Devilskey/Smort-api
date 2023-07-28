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
            _logger.Log(LogLevel.Information, "Started Token Background services");
            await ManageBlackList(stoppingToken);
        }

        private static async Task ManageBlackList(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                foreach (JWTtokenBlacklistItem blacklistItem in JWTTokenHandler.BlackList!)
                {
                    if(blacklistItem.ExpireTime < DateTime.Now)
                    {
                        JWTTokenHandler.BlackList.Remove(blacklistItem);
                    }
                }
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Consume Scoped Service Hosted Service is stopping.");

            await base.StopAsync(stoppingToken);
        }
    }
}
