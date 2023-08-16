using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Object.Security
{
    public class AESObject
    {
        public string? CipherText { get; set; }
        public string? Iv { get; set; }
        public string? Key { get; set; }
    }
}
