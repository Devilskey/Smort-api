using Smort_api.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Handlers
{
    public class ChunkHandler 
    {
        private string _folderNameTemp = "";
        private string _folderNameFiles = "";
        private string _extensionType = "";

        public ChunkHandler(string tempfolder, string filesFolder, string extensionType)
        {
            _folderNameTemp = tempfolder;
            _folderNameFiles = filesFolder;
            _extensionType = extensionType;
        }

        public bool AreAllChunksIn(string filename, int Chunksmax)
        {
            for (int i = 0; i < Chunksmax; i++)
            {
                if (!File.Exists($"{_folderNameTemp}/{filename + i}.{_extensionType}"))
                {
                    return false;
                }
            }
            return true;
        }

        public byte[] GetChunkFileData(string filename)
        {
            if (!Directory.Exists($"{_folderNameTemp}"))
                Directory.CreateDirectory($"{_folderNameTemp}");

            return File.ReadAllBytes($"{_folderNameTemp}/{filename}.{_extensionType}");
        }

        public void SaveFile(byte[] videoBytes, string filename, string id)
        {
            if (!Directory.Exists($"{_folderNameFiles}"))
                Directory.CreateDirectory($"{_folderNameFiles}");

            if (!Directory.Exists($"{_folderNameFiles}/{id}"))
                Directory.CreateDirectory($"{_folderNameFiles}/{id}");

            using (FileStream video = new FileStream($"{_folderNameFiles}/{id}/{filename}.{_extensionType}", FileMode.Append, FileAccess.Write))
            {
                video.Write(videoBytes, 0, videoBytes.Length);
            }
        }

        public void SaveFileChunk(byte[] videoBytes, string filename)
        {
            if (!Directory.Exists($"{_folderNameTemp}"))
                Directory.CreateDirectory($"{_folderNameTemp}");

            using (FileStream video = new FileStream($"{_folderNameTemp}/{filename}.{_extensionType}", FileMode.Append, FileAccess.Write))
            {
                video.Write(videoBytes, 0, videoBytes.Length);
            }
        }

        public void TempFileCleanup(string filename, int Chunksmax)
        {
            for (int i = 0; i < Chunksmax; i++)
            {
                if (!File.Exists($"{_folderNameTemp}/{filename + i}.{_extensionType}"))
                {
                    File.Delete($"{_folderNameTemp}/{filename + i}.{_extensionType}");
                }
            }
        }
    }
}
