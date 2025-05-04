using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using SixLabors.ImageSharp.Formats.Webp;
using System.Globalization;
using Tiktok_api.Settings_Api;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace Smort_api.Handlers
{

    /// <summary>
    /// https://github.com/rosenbjerg/FFMpegCore/tree/main
    /// 
    /// </summary>
    public static class VideoHandler
    {

        public static async Task ChangeSizeOfVideo(string input, string output, string FileName, int Sizewidth)
        {
            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            var info = await FFProbe.AnalyseAsync(input);

            int width = info.PrimaryVideoStream.Width;
            int height = info.PrimaryVideoStream.Height;

            int newheight = (int)Math.Round(width * (Sizewidth / (double)height));

            if (newheight % 2 != 0)
                newheight++;

            Console.WriteLine(output);
            Console.WriteLine(output);

            Console.WriteLine($"{info.PrimaryVideoStream.Width}{info.PrimaryVideoStream.Height}");

            Console.WriteLine($"{Sizewidth}{newheight}");

            await FFMpegArguments
                .FromFileInput(input)
                .OutputToFile(output + FileName, true, options => options
                    .WithVideoCodec("libx265")
                    .WithConstantRateFactor(23)
                    .WithCustomArgument("-pix_fmt yuv420p")
                    .WithCustomArgument("-err_detect ignore_err")
                    .ForceFormat("mp4")
                    .WithVideoFilters(filterOptions => filterOptions
                    .Scale(Sizewidth, newheight)))

                .ProcessAsynchronously();
        }
        public static async Task CreateThumbnails(string PathVideo, string PathThumbnails)
        {
            foreach (var sizes in ContentSizingObjects.Thumbnails)
            {
                await FFMpeg.SnapshotAsync(PathVideo, PathThumbnails + $"_{sizes.Size}.png", new System.Drawing.Size(sizes.Width, sizes.Width), TimeSpan.FromSeconds(10));
                
                using (Image image = Image.Load(PathThumbnails + $"_{sizes.Size}.png"))
                {

                    using (var outputMemoryStream = new MemoryStream())
                    {
                        image.Mutate(x => x.AutoOrient());
                        image.Save(PathThumbnails + $"_{sizes.Size}.webp", new WebpEncoder());
                    }
                }
                File.Delete(PathThumbnails + $"_{sizes.Size}.png");
            }

        }

    }
}
