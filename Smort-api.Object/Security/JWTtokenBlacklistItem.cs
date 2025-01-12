using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Object.Security
{
    public class JWTtokenBlacklistItem
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpireTime { get; set; }
    }
}
