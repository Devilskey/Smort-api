using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Object.ImagePosts
{

    public class CreateNewPostData
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; }
        public byte[] MediaData { get; set; }
        public ImageSize size { get; set; }
        public Guid GUIDObjSender { get; set; }
        public int? ChunkNumber { get; set; }
        public int? TotalChunks { get; set; }
    }
}
