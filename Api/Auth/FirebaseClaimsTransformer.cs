using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Tiktok_api.Auth
{
    public class FirebaseClaimsTransformer : IClaimsTransformation
    {
        private readonly IMemoryCache _cache;

        public FirebaseClaimsTransformer(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
                return Task.FromResult(principal);

            var firebaseUid = principal.FindFirst("firebase_uid")?.Value;
            var userId = principal.FindFirst("Id")?.Value;
            var roleValue = principal.FindFirst("role")?.Value;

            if (string.IsNullOrWhiteSpace(firebaseUid) || string.IsNullOrWhiteSpace(userId))
                return Task.FromResult(principal);

            var cacheKey = $"firebase-user-{firebaseUid}";
            if (!_cache.TryGetValue(cacheKey, out bool _))
            {
                var identity = new ClaimsIdentity();

                if (!principal.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));

                if (!principal.HasClaim(c => c.Type == ClaimTypes.Role) && !string.IsNullOrWhiteSpace(roleValue))
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));

                if (!principal.HasClaim(c => c.Type == "username") && principal.FindFirst("username") != null)
                    identity.AddClaim(new Claim("username", principal.FindFirst("username")!.Value));

                principal.AddIdentity(identity);
                _cache.Set(cacheKey, true, TimeSpan.FromMinutes(30));
            }

            return Task.FromResult(principal);
        }
    }
}
