using Smort_api.Object.DTO;

namespace Tiktok_api.Services;

public interface INotificationServices
{
    Task RegisterFcm(int userId, string fcmToken);
    Task<IEnumerable<InboxFeedDto>> GetInboxFeed(int userId);
    
    Task InboxSetSeen(int userId);
}