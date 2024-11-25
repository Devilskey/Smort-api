using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smort_api.Handlers
{
    public static class ImageHandler
    {
        public static void SaveThumbnail(byte[] ImageBytes, string filename, string id)
        {
            if (!Directory.Exists($"./Videos"))
                Directory.CreateDirectory($"./Videos");

            if (!Directory.Exists($"./Videos/{id}"))
                Directory.CreateDirectory($"./Videos/{id}");

            using (FileStream Image = new FileStream($"./Videos/{id}/{filename}", FileMode.Create))
            {

                Image.Write(ImageBytes);
                
            }
        }

        public static void SaveProfilePictures(byte[] ImageBytes, string filename)
        {
            if (!Directory.Exists($"./ProfilePictures"))
                Directory.CreateDirectory($"./ProfilePictures");


            using (FileStream Image = new FileStream($"./ProfilePictures/{filename}", FileMode.Create))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(Image, System.Text.Encoding.Default, false))
                {
                    binaryWriter.Write(ImageBytes);
                }
            }
        }
    }
}
