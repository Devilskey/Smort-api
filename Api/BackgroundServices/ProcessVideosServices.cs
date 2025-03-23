using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Smort_api.Handlers;
using Smort_api.Object.Security;
using System.Collections.Concurrent;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Tiktok_api.Settings_Api;
using Smort_api.Object.Video;
using Microsoft.AspNetCore.SignalR;
using Tiktok_api.SignalRHubs;
using FFMpegCore;

namespace Tiktok_api.BackgroundServices
{

    public class ProcessVideoServices : BackgroundService
    {
        private readonly ILogger<ProcessVideoServices> _logger;

        private readonly ConcurrentQueue<VideoToProcessObject> _VideosToProcess;

        private readonly NotificationHubHandler _notificationHub;

        public ProcessVideoServices(ILogger<ProcessVideoServices> logger, NotificationHubHandler notificationHub)
        {
            _VideosToProcess = new ConcurrentQueue<VideoToProcessObject>();
            _notificationHub = notificationHub;
            _logger = logger;
        }

        public void AddToQueue(VideoToProcessObject item)
        {
            _logger.LogInformation($"{item.FileName} Added to the processing list" );
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

            VideoHandler.CreateThumbnails(Video.Input, Video.Output + Video.FileName);
            _logger.LogInformation($"Thumbnail Done");

            using DatabaseHandler databaseHandler = new DatabaseHandler();

            int ThumbnailID = 0;

            MySqlCommand GetAndAddThumbnail = new MySqlCommand();
            GetAndAddThumbnail.CommandText =
                @"INSERT INTO File (File_Name, File_Location, Created_At) VALUES (@Name, @Location, @Created); 
                      SELECT LAST_INSERT_ID();";

            GetAndAddThumbnail.Parameters.AddWithValue("@Name", $"{Video.FileName}.png");
            GetAndAddThumbnail.Parameters.AddWithValue("@Location", $"{Video.Output}{Video.FileName}");
            GetAndAddThumbnail.Parameters.AddWithValue("@Created", DateTime.Now);

            ThumbnailID = databaseHandler.GetNumber(GetAndAddThumbnail);

            GetAndAddThumbnail.Dispose();

            using MySqlCommand InsertFileAndVideo = new MySqlCommand();

            InsertFileAndVideo.CommandText =
                @"INSERT INTO File (File_Name, File_location, Created_At, Deleted_At) VALUES (@FileName, @FileLocation, @CreatedAt, @DeletedAt);
                      INSERT INTO Content (User_Id, File_Id, Type, Description, Thumbnail, Created_At, Updated_At, Deleted_At) VALUES (@Id, LAST_INSERT_ID(), @Type, @Description, @Thumbnail, @CreatedAt, @UpdatedAt, @DeletedAt);";

            InsertFileAndVideo.Parameters.AddWithValue("@FileName", $"{Video.FileName}.mkv");
            InsertFileAndVideo.Parameters.AddWithValue("@Id", Video.UserId);
            InsertFileAndVideo.Parameters.AddWithValue("@FileLocation", $"{Video.Output}{Video.FileName}");
            InsertFileAndVideo.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            InsertFileAndVideo.Parameters.AddWithValue("@DeletedAt", DateTime.Now);
            InsertFileAndVideo.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);

            InsertFileAndVideo.Parameters.AddWithValue("@Type", "vid");

            InsertFileAndVideo.Parameters.AddWithValue("@Thumbnail", ThumbnailID.ToString());

            InsertFileAndVideo.Parameters.AddWithValue("@Description", Video.Description);
            databaseHandler.EditDatabase(InsertFileAndVideo);
            InsertFileAndVideo.Dispose();

            await _notificationHub.SendNotificationVideoToUser(Video.UserId, "Video has been uploaded");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.Log(LogLevel.Information, "Stopped video processing background service");
            await base.StopAsync(stoppingToken);
        }
    }
}
