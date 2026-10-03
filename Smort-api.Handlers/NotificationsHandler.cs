using System.Data;
using Dapper;

namespace Smort_api.Handlers;

using FirebaseAdmin.Messaging;

public class NotificationsHandler (IDbConnection _db)
{

    public async Task SendNotificationFollowers(string userId, string title, string body)
    {
        var sql = "select FcmCode, Id from Users_Public inner join Following on User_Id_Follower=Id where User_Id_Followed=@userId;";

        var notifyData = await _db.QueryAsync<NotifyFollowerBasicData>(sql, new { userId = userId });

        foreach (var notify in notifyData.ToList())
        {
            try
            {
                var message = new Message
                {
                    Token = notify.FcmCode,
                    Data = new Dictionary<string, string>
                    {
                        { "title", title },
                        { "body", body }
                    }
                };
                var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                
                await SaveNotificationToInbox(notify.Id.ToString(), userId, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
    
    public async Task SendNotification(string userId, string? userFromId , string title, string body)
    {
        var sql = "select FcmCode from Users_Public where Id = @userId;";
        var fcmCode = await _db.ExecuteScalarAsync<string>(sql, new {userId=userId  });

        try
        {
            var message = new Message
            {
                Token = fcmCode,
                Data = new Dictionary<string, string>
                {
                    { "title", title },
                    { "body", body }
                }
            };

            var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            await SaveNotificationToInbox(userId, userFromId, body);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    public async Task SaveNotificationToInbox(string userForId, string? userFromId, string body)
    {
        var sql = "insert into User_Notifications (Notification_For_User, Notification_From_User, Text) values (@Notification_For_User, @Notification_From_User, @Text); ";
        await _db.QueryAsync(sql, new { Notification_For_User = userForId, Notification_From_User = userFromId, Text = body });
    
    }
    
    public class NotifyFollowerBasicData 
    {
        public string FcmCode {get; set;}
        public int Id {get; set;}
    }
}