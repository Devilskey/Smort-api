using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Object
{
    public class AESObject
    {
        public byte[] cipherText { get; set; }
        public byte[] Iv { get; set; }
        public byte[] Key { get; set; }

    }
}
