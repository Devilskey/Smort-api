using System.Data;
using Dapper;
using Smort_api.Object.DTO;

namespace Tiktok_api.Services;

public class NotificationServices
    (IDbConnection _db): INotificationServices {
    
    public async Task RegisterFcm(int userId, string FcmCode)
    {
        var sql = "update Users_Public set FcmCode=@FcmCode where Id=@UserId;" ;

        await _db.QueryAsync(sql, new {FcmCode=FcmCode, userId=userId});
        
        return;
    }

    public async Task<IEnumerable<InboxFeedDto>> GetInboxFeed(int userId)
    { 
        var sql = "select Id, Notification_From_User, Text, Has_Seen, Created_At From User_Notifications where Notification_For_User=@UserId; " ;

        var inbox = await _db.QueryAsync<InboxFeedDto>(sql, new { userId=userId});

        return inbox;
    }

    public async Task InboxSetSeen(int userId)
    {
        var sql = "update User_Notifications set Has_Seen=1 where Notification_For_User=@userId and Has_Seen=0;" ;

        await _db.QueryAsync(sql, new {userId=userId});
    }
}