using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Handlers
{
    public static class VideoFileHandler
    {
        public static void SaveVideo(byte[] videoBytes, string filename, string id)
        {
            if (!Directory.Exists($"./Videos/{id}")) { 
                Directory.CreateDirectory($"./Videos/{id}");
            }


            using (FileStream video = new FileStream($"./Videos/{id}/{filename}", FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(video, System.Text.Encoding.Default, false))
                {
                    binaryWriter.Write(videoBytes);
                }
            }
        }
    }
}
