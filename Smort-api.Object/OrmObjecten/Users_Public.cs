using Smort_api.Object.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Object.OrmObjecten
{
    public class Users_Public : IDatabaseModel
    {
        public int Id { get; set; }
        public int Person_Id { get; set; }
        public int Profile_Picture {  get; set; }
        public bool IsAllowed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
