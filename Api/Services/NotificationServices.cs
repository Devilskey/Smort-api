using System.Data;
using Dapper;

namespace Tiktok_api.Services;

public class NotificationServices
    (IDbConnection _db): INotificationServices {
    
    public async Task RegisterFcm(int userId, string FcmCode)
    {
        var sql = "update Users_Public set FcmCode=@FcmCode where Id=@UserId;" ;

        await _db.QueryAsync(sql, new {FcmCode=FcmCode, userId=userId});
        
        return;
    }
}