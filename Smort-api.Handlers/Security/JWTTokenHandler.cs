using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Smort_api.Object;
using Smort_api.Object.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Smort_api.Handlers
{
    public static class JWTTokenHandler
    {
        private const string TokenSecret = "IWANTTOSETHERAINBOWHIGHINTHESKYIWANTTOSOYOUANDMEONABIRDFLYAWAY";
        private static readonly TimeSpan TokenLifeTime = TimeSpan.FromHours(8);

        /// <summary>
        /// List of tokens from deleted accounts
        /// </summary>
        public static List<JWTtokenBlacklistItem>? BlackList { get; set; } = new List<JWTtokenBlacklistItem>();

        public static string GenerateToken(LoginObject loginDetails, string id)
        {
            JwtSecurityTokenHandler tokenhandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(TokenSecret);

            var claims = new List<Claim>
            {
                new("Id", id!),
                new(JwtRegisteredClaimNames.Email, loginDetails.Email!),
                new("TimeCreated", DateTime.Now.ToString())
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

    }
}
