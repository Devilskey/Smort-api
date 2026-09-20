namespace Tiktok_api.Services;

public interface INotificationServices
{
    Task RegisterFcm(int userId, string fcmToken);
}