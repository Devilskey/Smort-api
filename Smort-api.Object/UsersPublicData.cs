using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Object
{
    public class UsersPublicData
    {
        public int Id { get; set; }
        public int PrivateUserId { get; set; }
        public string Username { get; set; }
        public Blob Profile_Picture { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set;}
        public DateTime DeletedAt { get; set; }
    }
}
