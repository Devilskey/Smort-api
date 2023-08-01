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

            JWTTokenHandler.BlackList = ReadBlackList();

            await ManageBlackList(stoppingToken);
        }

        private async Task ManageBlackList(CancellationToken stoppingToken)
        {
            List<JWTtokenBlacklistItem> blacklistItemsToRemove = new List<JWTtokenBlacklistItem>();

            while (!stoppingToken.IsCancellationRequested)
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
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.Log(LogLevel.Information, "stopped Token Blacklist management services");
            WriteBlackList();
            await base.StopAsync(stoppingToken);
        }

        private List<JWTtokenBlacklistItem> ReadBlackList()
        {
            string json = File.ReadAllText("BlackList.json");
            return JsonConvert.DeserializeObject<List<JWTtokenBlacklistItem>>(json);
        }

        private void WriteBlackList()
        {
            string json = JsonConvert.SerializeObject(JWTTokenHandler.BlackList);
            File.WriteAllText("BlackList.json", json);
        }
    }
}
