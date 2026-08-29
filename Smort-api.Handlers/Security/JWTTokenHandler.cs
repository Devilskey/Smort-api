using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Smort_api.Object;
using Smort_api.Object.Database;
using Smort_api.Object.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace Smort_api.Handlers
{
    public static class JWTTokenHandler
    {
        private static string TokenSecret = Environment.GetEnvironmentVariable("SecretTokenJWT") ?? "IWANTTOSETHERAINBOWHIGHINTHESKYIWANTTOSOYOUANDMEONABIRDFLYAWAYTESTINGITAGAIN";
        private static readonly TimeSpan TokenLifeTime = TimeSpan.FromHours(8);

        /// <summary>
        /// List of tokens from deleted accounts
        /// </summary>
        public static List<JWTtokenBlacklistItem> BlackList { get; set; } = new List<JWTtokenBlacklistItem>();

        public static bool IsBlacklisted(string token)
        {
            if (BlackList == null)
                return false;

            token = ExtractBearerToken(token);

            foreach (JWTtokenBlacklistItem blacklistItem in BlackList!)
            {
                if (blacklistItem.Token == token)
                {
                    return true;
                }
            }
            return false;
        }

        public static string? ExtractBearerToken(string? rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
                return null;

            var parts = rawToken.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                return parts[1];

            return rawToken;
        }


        public static string GenerateToken(LoginObject loginDetails, string id, string username, int role)
        {
            JwtSecurityTokenHandler tokenhandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(TokenSecret);

            var claims = new List<Claim>
            {
                new("id", id!),
                new("username", username!),
                new("role", role.ToString()!),
                new(ClaimTypes.NameIdentifier, id),
                new(JwtRegisteredClaimNames.Email, loginDetails.Email!),
                new("timeCreated", DateTime.Now.ToString()),
            };
            SymmetricSecurityKey securiyKey= new SymmetricSecurityKey(key);

            SigningCredentials Credentials = new SigningCredentials(securiyKey, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(8),
                Issuer = "http://localhost",
                Audience = "http://localhost",
                SigningCredentials = Credentials
            };

            SecurityToken token = tokenhandler.CreateToken(tokenDescriptor);

            return tokenhandler.WriteToken(token);
        }

        public static List<JWTtokenBlacklistItem> ReadBlackList()
        {
            string filePath = Path.Combine(AppContext.BaseDirectory, "BlackList.json");
            try
            {
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, "[]");
                    return new List<JWTtokenBlacklistItem>();
                }

                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<JWTtokenBlacklistItem>>(json)
                       ?? new List<JWTtokenBlacklistItem>();
            }
            catch (IOException)
            {
                return new List<JWTtokenBlacklistItem>();
            }
            catch (JsonException)
            {
                return new List<JWTtokenBlacklistItem>();
            }
        }

        public static void WriteBlackList()
        {
            string json = JsonConvert.SerializeObject(BlackList);
            File.WriteAllText("BlackList.json", json);
        }
    }
}
