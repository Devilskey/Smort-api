namespace Smort_api.Handlers;

using FirebaseAdmin.Messaging;

public class NotificationsHandler
{
    public async Task SendNotification(string userId, string title, string body)
    {
        try
        {
            var message = new Message
            {
                Token = "",
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}