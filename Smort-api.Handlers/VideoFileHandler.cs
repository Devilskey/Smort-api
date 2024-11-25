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

        public static void TempFileCleanup(string filename, int Chunksmax)
        {
            for (int i = 0; i < Chunksmax; i++)
            {
                if (!File.Exists($"./temp/{filename + i}.mkv"))
                {
                    File.Delete($"./temp/{filename + i}.mkv");
                }
            }
        }
        public static bool AreAllChunksIn(string filename, int Chunksmax)
        {
            for (int i = 0; i < Chunksmax; i++)
            {
                if (!File.Exists($"./temp/{filename + i}.mkv"))
                {
                    return false;
                }
            }
            return true;
        }

        public static byte[] GetChunkFileData(string filename)
        {
            if (!Directory.Exists($"./temp"))
                Directory.CreateDirectory($"./temp");

            return File.ReadAllBytes($"./temp/{filename}.mkv");
        }

        public static void SaveVideoChunk(byte[] videoBytes, string filename)
        {
            if (!Directory.Exists($"./temp"))
                Directory.CreateDirectory($"./temp");

            using (FileStream video = new FileStream($"./temp/{filename}.mkv", FileMode.Append, FileAccess.Write))
            {
                video.Write(videoBytes, 0, videoBytes.Length);
            }
        }

        public static void SaveVideo(byte[] videoBytes, string filename, string id)
        {
            if (!Directory.Exists($"./Videos"))
                Directory.CreateDirectory($"./Videos");

            if (!Directory.Exists($"./Videos/{id}")) 
                Directory.CreateDirectory($"./Videos/{id}");

            using (FileStream video = new FileStream($"./Videos/{id}/{filename}.mkv", FileMode.Append, FileAccess.Write))
            {
                video.Write(videoBytes, 0, videoBytes.Length);
            }
        }
    }
}
