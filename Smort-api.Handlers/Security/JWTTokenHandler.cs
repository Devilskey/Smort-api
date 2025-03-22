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

            foreach (JWTtokenBlacklistItem blacklistItem in BlackList!)
            {
                if(blacklistItem.Token == token)
                {
                    return true;
                }
            }
            return false;
        }


        public static string GenerateToken(LoginObject loginDetails, string id, Roll roll)
        {
            JwtSecurityTokenHandler tokenhandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(TokenSecret);

            var claims = new List<Claim>
            {
                new("Id", id!),
                new(ClaimTypes.NameIdentifier, id),
                new(JwtRegisteredClaimNames.Email, loginDetails.Email!),
                new("TimeCreated", DateTime.Now.ToString()),
                new("Roll", roll.ToString()),
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

        public static List<JWTtokenBlacklistItem>? ReadBlackList()
        {
            try
            {
                string json = File.ReadAllText("BlackList.json");
                return JsonConvert.DeserializeObject<List<JWTtokenBlacklistItem>>(json)!;
            }
            catch (FileNotFoundException ex)
            {
                File.WriteAllText("BlackList.json", "[]");
                return null;
            }
            
        }

        public static void WriteBlackList()
        {
            string json = JsonConvert.SerializeObject(BlackList);
            File.WriteAllText("BlackList.json", json);
        }
    }
}
