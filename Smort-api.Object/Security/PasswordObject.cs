using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Object.Security
{
    public class PasswordObject
    {
        public string? Password { get; set; }
        public string? Salt { get; set; }
    }
}
