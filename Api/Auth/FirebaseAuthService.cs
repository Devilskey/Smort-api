using Dapper;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Smort_api.Object.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Tiktok_api.Auth
{
    public interface IFirebaseAuthService
    {
        Task<FirebaseLoginResult> LoginWithFirebaseTokenAsync(string firebaseIdToken);
    }

    public class FirebaseLoginResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? ErrorMessage { get; set; }
        public LocalFirebaseUser? User { get; set; }
    }

    public class LocalFirebaseUser
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public bool? AllowedUser { get; set; }
        public int Role { get; set; }
        public string? FirebaseUid { get; set; }
        public string? Email { get; set; }
    }

    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly MySqlConnection _connection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FirebaseAuthService> _logger;
        private readonly string _firebaseProjectId;
        private readonly string _issuer;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public FirebaseAuthService(MySqlConnection connection, IConfiguration configuration, ILogger<FirebaseAuthService> logger)
        {
            _connection = connection;
            _configuration = configuration;
            _logger = logger;

            _firebaseProjectId = _configuration["Firebase:ProjectId"] ?? Environment.GetEnvironmentVariable("FirebaseProjectId") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(_firebaseProjectId))
                throw new InvalidOperationException("Firebase:ProjectId configuration is required for Firebase authentication.");

            _issuer = $"https://securetoken.google.com/{_firebaseProjectId}";
            _jwtKey = Environment.GetEnvironmentVariable("SecretTokenJWT") ?? _configuration["JwtSettings:Key"]!;
            _jwtIssuer = _configuration["JwtSettings:Issuer"] ?? "SmortApi";
            _jwtAudience = _configuration["JwtSettings:Audience"] ?? "SmortApiUsers";
        }

        public async Task<FirebaseLoginResult> LoginWithFirebaseTokenAsync(string firebaseIdToken)
        {
            if (string.IsNullOrWhiteSpace(firebaseIdToken))
            {
                return new FirebaseLoginResult { Success = false, ErrorMessage = "Firebase token is required." };
            }

            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _firebaseProjectId },
                };

                payload = await GoogleJsonWebSignature.ValidateAsync(firebaseIdToken, settings);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Firebase token validation failed.");
                return new FirebaseLoginResult { Success = false, ErrorMessage = "Firebase token validation failed." };
            }

            if (string.IsNullOrWhiteSpace(payload.Subject) || string.IsNullOrWhiteSpace(payload.Email))
            {
                return new FirebaseLoginResult { Success = false, ErrorMessage = "Firebase token did not contain an email or uid." };
            }

            await EnsureConnectionOpenAsync();

            var user = await GetOrCreateLocalUserAsync(payload);
            if (user == null)
            {
                return new FirebaseLoginResult { Success = false, ErrorMessage = "Unable to create or resolve local user record." };
            }

            if (user.AllowedUser == false)
            {
                return new FirebaseLoginResult { Success = false, ErrorMessage = "User is not allowed to sign in yet." };
            }

            var token = GenerateLocalJwtToken(user, payload);
            return new FirebaseLoginResult { Success = true, Token = token, User = user };
        }

        private async Task EnsureConnectionOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();
        }

        private async Task<LocalFirebaseUser?> GetOrCreateLocalUserAsync(GoogleJsonWebSignature.Payload payload)
        {
            const string query = @"
                SELECT u.Id, u.Username, u.AllowedUser, p.Role_Id AS Role, p.Firebase_Uid AS FirebaseUid, p.Email AS Email
                FROM Users_Private p
                JOIN Users_Public u ON u.Person_Id = p.Id
                WHERE p.Firebase_Uid = @FirebaseUid OR p.Email = @Email
                LIMIT 1;";

            var user = await _connection.QueryFirstOrDefaultAsync<LocalFirebaseUser>(query, new
            {
                FirebaseUid = payload.Subject,
                Email = payload.Email
            });

            if (user != null)
            {
                if (string.IsNullOrWhiteSpace(user.FirebaseUid))
                {
                    const string updateFirebaseUid = @"
                        UPDATE Users_Private
                        SET Firebase_Uid = @FirebaseUid
                        WHERE Email = @Email;";

                    await _connection.ExecuteAsync(updateFirebaseUid, new { FirebaseUid = payload.Subject, Email = payload.Email });
                }

                const string updateLoginTime = @"
                    UPDATE Users_Private
                    SET Last_Login_At = @LastLoginAt
                    WHERE Firebase_Uid = @FirebaseUid OR Email = @Email;";

                await _connection.ExecuteAsync(updateLoginTime, new { LastLoginAt = DateTime.UtcNow, FirebaseUid = payload.Subject, Email = payload.Email });
                return user;
            }

            var baseUsername = BuildUsername(payload);
            var profilePicId = await CreateDefaultProfilePictureAsync();
            var publicUsername = await EnsureUniqueUsernameAsync(baseUsername);

            const string insertPrivate = @"
                INSERT INTO Users_Private (Role_Id, Email, Firebase_Uid, Provider, Last_Login_At)
                VALUES (@RoleId, @Email, @FirebaseUid, @Provider, @LastLoginAt);
                SELECT LAST_INSERT_ID();";

            var personId = await _connection.ExecuteScalarAsync<int>(insertPrivate, new
            {
                RoleId = 1,
                Email = payload.Email,
                FirebaseUid = payload.Subject,
                Provider = "firebase",
                LastLoginAt = DateTime.UtcNow
            });

            const string insertPublic = @"
                INSERT INTO Users_Public (Person_Id, Username, Profile_Picture, Created_At, Updated_At, Deleted_At, AllowedUser)
                VALUES (@PersonId, @Username, @ProfilePicture, @CreatedAt, @UpdatedAt, @DeletedAt, @AllowedUser);
                SELECT LAST_INSERT_ID();";

            var publicId = await _connection.ExecuteScalarAsync<int>(insertPublic, new
            {
                PersonId = personId,
                Username = publicUsername,
                ProfilePicture = profilePicId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DeletedAt = (DateTime?)null,
                AllowedUser = false
            });

            return new LocalFirebaseUser
            {
                Id = publicId,
                UserName = publicUsername,
                AllowedUser = false,
                Role = 1,
                FirebaseUid = payload.Subject,
                Email = payload.Email
            };
        }

        private async Task<int> CreateDefaultProfilePictureAsync()
        {
            const string insertFileSql = @"
                INSERT INTO File_Image (File_Name, File_location, file_type_Id, Created_At)
                VALUES (@FileName, @FileLocation, 4, @CreatedAt);
                SELECT LAST_INSERT_ID();";

            var guid = Guid.NewGuid().ToString();
            return await _connection.ExecuteScalarAsync<int>(insertFileSql, new
            {
                FileName = $"firebase_{guid}.webp",
                FileLocation = $"./ProfilePictures/firebase_{guid}.webp",
                CreatedAt = DateTime.UtcNow
            });
        }

        private async Task<string> EnsureUniqueUsernameAsync(string baseUsername)
        {
            const string selectCountSql = @"SELECT COUNT(*) FROM Username_Counter WHERE Username = @Username;";
            const string insertUsernameCounter = @"
                INSERT INTO Username_Counter (Username, Amount, Created_At, Updated_At)
                VALUES (@Username, @Amount, @CreatedAt, @UpdatedAt);";
            const string selectAmountSql = @"SELECT Amount FROM Username_Counter WHERE Username = @Username;";
            const string updateAmountSql = @"
                UPDATE Username_Counter
                SET Amount = @Amount, Updated_At = @UpdatedAt
                WHERE Username = @Username;";

            var currentCount = await _connection.ExecuteScalarAsync<int>(selectCountSql, new { Username = baseUsername });
            if (currentCount == 0)
            {
                await _connection.ExecuteAsync(insertUsernameCounter, new
                {
                    Username = baseUsername,
                    Amount = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                return $"{baseUsername}#0001";
            }

            var amount = await _connection.ExecuteScalarAsync<int>(selectAmountSql, new { Username = baseUsername });
            amount++;

            await _connection.ExecuteAsync(updateAmountSql, new
            {
                Username = baseUsername,
                Amount = amount,
                UpdatedAt = DateTime.UtcNow
            });

            return $"{baseUsername}#{amount:D4}";
        }

        private string BuildUsername(GoogleJsonWebSignature.Payload payload)
        {
            if (!string.IsNullOrWhiteSpace(payload.Email))
            {
                var prefix = payload.Email.Split('@')[0];
                return prefix.Length > 24 ? prefix.Substring(0, 24) : prefix;
            }

            return payload.Subject?.Substring(0, Math.Min(payload.Subject.Length, 24)) ?? "firebaseuser";
        }

        private string GenerateLocalJwtToken(LocalFirebaseUser user, GoogleJsonWebSignature.Payload payload)
        {
            var claims = new List<Claim>
            {
                new Claim("Id", user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("firebase_uid", payload.Subject ?? string.Empty),
                new Claim(ClaimTypes.Email, payload.Email ?? string.Empty),
                new Claim("username", user.UserName ?? string.Empty),
                new Claim("role", user.Role.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var jwt = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
