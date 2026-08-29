using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Smort_api.Object.Videos;
using Tiktok_api.Settings_Api;
using Tiktok_api.Services;

namespace Tiktok_api.Controllers
{
    [ApiController]
    public class Images : ControllerBase
    {
        private readonly ILogger Logger;
        private readonly IImageService _imageService;

        public Images(ILogger<Images> logger, IImageService imageService)
        {
            Logger = logger;
            _imageService = imageService;
        }

        [Route("Images/GetUsersProfileImage")]
        [HttpGet]
        public async Task<ActionResult?> GetUsersProfileImage(int UserId, Sizes size = Sizes.M)
        {
            try
            {
                var path = await _imageService.GetProfileImagePathAsync(UserId);
                if (string.IsNullOrEmpty(path)) return BadRequest();

                FileStream filestream = null;
                Logger.LogInformation(size.ToString());

                try
                {
                    filestream = System.IO.File.OpenRead(path + $"_{size}.webp");
                }
                catch (Exception)
                {
                    filestream = System.IO.File.OpenRead(path);
                    Console.Write($"Returning old image Formate 1000X1000 {path}");
                }
                return File(filestream, contentType: "image/*", enableRangeProcessing: true);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        //WARNING THIS IS MADE WITH DUCKTAPE AND HOPE THIS WILL BREAKE DOWN SOME DAY UPDATE REQUIRED
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ImageId"></param>
        /// <returns></returns>
        [Route("Images/GetImage")]
        [HttpGet]
        public async Task<ActionResult?> GetImage(int ImageId, Sizes size = Sizes.M, bool IsContent = true)
        {
            try
            {
                var path = await _imageService.GetImagePathAsync(ImageId, IsContent);
                if (string.IsNullOrEmpty(path)) return BadRequest();

                FileStream filestream = null;
                Logger.LogInformation(size.ToString());

                try
                {
                    filestream = System.IO.File.OpenRead(path + $"_{size}.webp");
                }catch(Exception)
                {
                    filestream = System.IO.File.OpenRead(path);
                    Console.Write($"Returning old image Formate 1000X1000 {path}");
                }
                return File(filestream, contentType: "image/*", enableRangeProcessing: true);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                return BadRequest();
            }
        }
    }
}
