﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace Smort_api.Handlers
{
    public static class ImageHandler
    {
        public static byte[]? ChangeSizeOfImage(byte[] imgMediaData, int width, int height)
        {
            using (MemoryStream inputMemoryStream = new MemoryStream(imgMediaData))
            {
                using (Image image = Image.Load(inputMemoryStream))
                {
                    float sourceWidth = image.Width;
                    float sourceHeight = image.Height;
                    float nPercentW = (float)width / sourceWidth;
                    float nPercentH = (float)height / sourceHeight;
                    float nPercent = Math.Min(nPercentW, nPercentH);

                    int destWidth = (int)(sourceWidth * nPercent);
                    int destHeight = (int)(sourceHeight * nPercent);

                    image.Mutate(x => x.Resize(destWidth, destHeight, KnownResamplers.Bicubic));

                    using (var outputMemoryStream = new MemoryStream())
                    {
                        image.Mutate(x => x.AutoOrient());
                        image.Save(outputMemoryStream, new WebpEncoder());
                        return outputMemoryStream.ToArray();
                    }
                }
            }
        }
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
