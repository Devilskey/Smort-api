using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object.Security;
using System.Collections.Concurrent;
using System.Data;
using Dapper;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Tiktok_api.Settings_Api;
using Smort_api.Object.Video;
using Microsoft.AspNetCore.SignalR;
using Tiktok_api.SignalRHubs;

namespace Tiktok_api.BackgroundServices
{

    public class ProcessVideoServices : BackgroundService
    {
        private readonly ILogger<ProcessVideoServices> _logger;

        private readonly ConcurrentQueue<VideoToProcessObject> _VideosToProcess;

        private readonly IDbConnection _db;

        private readonly NotificationHubHandler _notificationHub;

        public ProcessVideoServices(ILogger<ProcessVideoServices> logger, NotificationHubHandler notificationHub, IDbConnection db)
        {
            _VideosToProcess = new ConcurrentQueue<VideoToProcessObject>();
            _notificationHub = notificationHub;
            _logger = logger;
            _db = db;   
        }

        public void AddToQueue(VideoToProcessObject item)
        {
            _logger.LogInformation($"{item.FileName} Added to the processing list");
            _VideosToProcess.Enqueue(item);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Log(LogLevel.Information, "Started video processing background service");

            await ManageBlackList(stoppingToken);
        }

        private async Task ManageBlackList(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_VideosToProcess.TryDequeue(out var video))
                {
                    _logger.LogInformation($"{video.FileName} Started to be processed");

                    SaveVideoToDatabase(video);

                    _logger.LogInformation($"{video.FileName} Done processing the video");
                }
                await Task.Delay(1000, stoppingToken);
            }
        }


        public async void SaveVideoToDatabase(VideoToProcessObject Video)
        {
            //Video Resize 
            foreach (var size in ContentSizingObjects.Content)
            {
                await VideoHandler.ChangeSizeOfVideo(Video.Input, Video.Output, Video.FileName + $"_{size.Size}.mp4", size.Width);
                _logger.LogInformation($"{size.Size} Done");

            }
            _logger.LogInformation($"Thumbnail Start");

            await VideoHandler.CreateThumbnails(Video.Input, Video.Output + Video.FileName);
            _logger.LogInformation($"Thumbnail Done");
            
            int thumbnailID = 0;

           var sqlGetAndAddThumbnail =
                @"INSERT INTO File_Image (File_Name, File_Location, file_type_Id, Created_At) VALUES (@Name, @Location, 2, @Created); 
                      SELECT LAST_INSERT_ID();";
           
            thumbnailID = await _db.ExecuteScalarAsync<int>(sqlGetAndAddThumbnail, new
            {
                Name = $"{Video.FileName}.png", 
                Location= $"{Video.Output}{Video.FileName}",
                Created= DateTime.Now
            });


            var sqlInsertFileAndVideo =
                @"
                INSERT INTO Content (User_Id, Type, Description, Thumbnail, Created_At, Updated_At, Deleted_At) VALUES (@Id, @Type, @Description, @Thumbnail, @CreatedAt, @UpdatedAt, @DeletedAt);
                INSERT INTO File_Content (File_Name, File_location,file_type_Id, Content_Id, Created_At, Deleted_At) VALUES (@FileName, @FileLocation, @fileType, LAST_INSERT_ID(), @CreatedAt, @DeletedAt);";
            
            _db.QueryAsync(sqlInsertFileAndVideo, new
            {
                FileName= $"{Video.FileName}.mkv",
                Id= Video.UserId,
                FileLocation= $"{Video.Output}{Video.FileName}",
                CreatedAt= DateTime.Now,
                DeletedAt= DateTime.Now,
                fileType= 3,
                UpdatedAt=DateTime.Now,
                Type="vid",
                Thumbnail=thumbnailID.ToString(),
                Description=Video.Description
            });

            await _notificationHub.SendNotificationVideoToUser(Video.UserId, "Video has been uploaded");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.Log(LogLevel.Information, "Stopped video processing background service");
            await base.StopAsync(stoppingToken);
        }
    }
}
