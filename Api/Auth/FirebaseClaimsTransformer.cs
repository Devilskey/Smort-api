using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using MySql.Data.MySqlClient;
using Serilog;
using System;
using System.Security.Claims;
using static Dapper.SqlMapper;

namespace Tiktok_api.Auth
{
    public class FirebaseClaimsTransformer
        (IMemoryCache cache, MySqlConnection connection, IConfiguration configuration, ILogger<FirebaseClaimsTransformer> logger) : IClaimsTransformation

    {
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            Log.Information("🔥 ClaimsTransformer EXECUTED");
            if (principal.Identity is not ClaimsIdentity identity ||
                !identity.IsAuthenticated)
            {
                Log.Information("🔥 ClaimsTransformer Failed");

                return principal;
            }

            var firebaseUid = principal.FindFirst("user_id")?.Value;

            if (string.IsNullOrWhiteSpace(firebaseUid))
            {
                Log.Information("🔥 ClaimsTransformer Whitespace");

                return principal;
            }

            var cacheKey = $"firebase-user-{firebaseUid}";

            Log.Information(cacheKey);

            var userData = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var query = """
            SELECT 
                Users_Public.Id AS id,
                Users_Private.Role_Id AS role,
                Users_Private.Email AS email,
                Users_Public.UserName AS username
            FROM Users_Private
            INNER JOIN Users_Public
                ON Users_Public.Person_Id = Users_Private.Id
            WHERE Users_Private.Firebase_Uid = @Firebase;
        """;

                var dbUser = await connection.QueryFirstOrDefaultAsync<DatabaseUserObject>(
                    query,
                    new { Firebase = firebaseUid });

                if (dbUser == null)
                    return null;

                Log.Information(dbUser.id.ToString());


                return new SmortIdentity
                {
                    FirebaseUid = firebaseUid,
                    UserIdPublic = dbUser.id,
                    Username =  dbUser.username,
                    Role = dbUser.role,
                    Email = dbUser.email
                };
            });

            if (userData == null)
                return principal;

            if (!identity.HasClaim(c => c.Type == "app_user_id"))
                identity.AddClaim(new Claim("app_user_id", userData.UserIdPublic.ToString()));

            if (!identity.HasClaim(c => c.Type == "app_role") && userData.Role != null)
                identity.AddClaim(new Claim(ClaimTypes.Role, userData.Role));

            if (!identity.HasClaim(c => c.Type == "email") && userData.Email != null)
                identity.AddClaim(new Claim("email", userData.Email));
                    
            if (!identity.HasClaim(c => c.Type == "Username") && userData.Username != null)
                identity.AddClaim(new Claim("Username", userData.Username));
            
            logger.LogInformation("works");
            return principal;
        }
    }

        public class DatabaseUserObject
    {
        public int id { get; set; }
        public string username { get; set; }
        public string role { get; set; }
        public string email { get; set; }

    }

    public class SmortIdentity
    {
        public string FirebaseUid { get; set; } = default!;
        public string Username { get; set; }

        public int UserIdPublic { get; set; }
        public string? Role { get; set; }
        public string? Email { get; set; }
    }
}