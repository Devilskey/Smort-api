using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Dapper;
using Serilog;

namespace Extensions
{
    public static class authExtension
    {
        public static IServiceCollection AddFirebaseAuth(this IServiceCollection service, IConfiguration configuration)
        {
            string projectId = configuration.GetValue<string>("Firebase:projectId") ?? throw new Exception("Setting Firebase:projectId is empty");


            var client = new HttpClient();
            var keys = client
                .GetStringAsync(
                    "https://www.googleapis.com/robot/v1/metadata/x509/securetoken@system.gserviceaccount.com").Result;
            var originalKeys = new JsonWebKeySet(keys).GetSigningKeys();
            var additionalkeys = client
                .GetStringAsync(
                    "https://www.googleapis.com/service_accounts/v1/jwk/securetoken@system.gserviceaccount.com").Result;
            var morekeys = new JsonWebKeySet(additionalkeys).GetSigningKeys();
            var totalkeys = originalKeys.Concat(morekeys);

            service.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.IncludeErrorDetails = true;
                    options.Authority = $"https://securetoken.google.com/{projectId}";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = $"https://securetoken.google.com/{projectId}",
                        ValidateAudience = true,
                        ValidAudience = projectId,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = totalkeys
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            // Receive the JWT token that firebase has provided
                            var firebaseToken = context.SecurityToken as Microsoft.IdentityModel.JsonWebTokens.JsonWebToken;
                            // Get the Firebase UID of this user
                            Log.Information("Logged");

                            var firebaseUid = firebaseToken?.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
                            var Email = firebaseToken?.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                            var firstName = firebaseToken?.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                            var Provider = firebaseToken?.Claims.FirstOrDefault(c => c.Type == "sign_in_provider")?.Value ?? "unknown" ;

                            var connectionString = configuration.GetValue<string>("Database:ConnectionString") ?? throw new Exception("MySql:ConnectionString missing in the settings");

                            var connection = new MySqlConnection(connectionString);
                            connection.Open();

                            var checkSql = """
                                            INSERT INTO Users_Private (
                                                Firebase_Uid,
                                                Email,
                                                Provider,
                                                Role_Id
                                            )
                                            SELECT
                                                @Firebase,
                                                @Email,
                                                @Provider,
                                                1
                                            WHERE NOT EXISTS (
                                                SELECT 1
                                                FROM Users_Private
                                                WHERE Firebase_Uid = @Firebase or Email = @Email
                                            );

                                            UPDATE Users_Private SET Firebase_Uid=@Firebase WHERE Email=@Email AND Firebase_Uid!=@Firebase AND ROW_COUNT()=0;
                                            
                                            INSERT INTO Users_Public (
                                                Person_Id,
                                                Created_At
                                            )
                                            SELECT
                                                up.Id,
                                                @Date
                                            FROM Users_Private up
                                            WHERE up.Firebase_Uid = @Firebase
                                              AND NOT EXISTS (
                                                  SELECT 1
                                                  FROM Users_Public pub
                                                  WHERE pub.Person_Id = up.Id
                                              );
                                            """;
                            Log.Information("Logged");

                            await connection.QueryAsync(checkSql, new { Firebase = firebaseUid, Email = Email, Provider = Provider, Date = DateTime.Now });
                            connection.Close();
                        }
                    };
                    });
            Log.Information("Firebase Auth Configured");

            return service;
        }
    }
}
