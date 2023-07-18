using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Object
{
    public class UserData
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string? Profile_Picture { get; set; }
    }
}
