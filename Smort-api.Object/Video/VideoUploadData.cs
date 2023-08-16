using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Object.Videos
{
    public class VideoUploadData
    {
        public string? FileName { get; set; }
        public byte[]? MediaData { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
