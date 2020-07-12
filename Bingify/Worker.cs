namespace Bingify
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private string SPOTLIGHT_FOLDER_PATH = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets";
        private string PICTURE_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        private FileSystemWatcher watcher;
        private const string JPEG_EXTENSION = ".jpg";
        private  string WALLPAPER_FOLDER = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\WallPapers";
        public Worker(ILogger<Worker> logger)
        {
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // check if directories exist - should have read permissions on these folders
            if (!Directory.Exists(SPOTLIGHT_FOLDER_PATH))
            {
                this.logger.LogError("Windows spotlight is not enabled");
                throw new Exception();
            }

            if (!Directory.Exists(PICTURE_FOLDER))
            {
                this.logger.LogError("Picture folder does not exist");
                throw new Exception();
            }

            if (!CheckIfUserHasReadWriteAccess(SPOTLIGHT_FOLDER_PATH, PICTURE_FOLDER))
            {
                this.logger.LogError($"Please check the user has access to the following folders: {SPOTLIGHT_FOLDER_PATH}, {PICTURE_FOLDER}");
                throw new Exception();
            }

            if (!Directory.Exists(WALLPAPER_FOLDER))
            {
                Directory.CreateDirectory(WALLPAPER_FOLDER);
            }
            

            if (this.watcher == null)
            {
                this.watcher = new FileSystemWatcher(SPOTLIGHT_FOLDER_PATH);
                this.watcher.Changed += this.WatcherFolderChanged;
                this.watcher.EnableRaisingEvents = true;
                this.logger.LogInformation("Bingify running at: {time}", DateTimeOffset.Now);
            }
            else
            {
                this.watcher.Changed -= this.WatcherFolderChanged;
                this.watcher = null;
            }


            await Task.CompletedTask;
            
        }

        private void WatcherFolderChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (e.ChangeType == WatcherChangeTypes.Changed && e.ChangeType != WatcherChangeTypes.Deleted)
                {
                    // new file been added/modified - we going to check if the jpg already exists before saving so this should not be an issue
                    var fileInfo = new FileInfo(e.FullPath);
                    if (!fileInfo.Exists)
                    {
                        this.logger.LogError($"Cant get created/modified file in the spotlight folder: {SPOTLIGHT_FOLDER_PATH}");
                        return;
                    }

                    var extention = Path.GetExtension(e.FullPath);
                    if (string.Compare(extention, JPEG_EXTENSION, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        this.logger.LogError($"{e.FullPath} is already a image file");
                        return;
                    }

                    // check file is potentially an image (going to use min size of 400kb for now)
                    if (fileInfo.Length < 400000)
                    {
                        this.logger.LogError($"{e.FullPath} is too small for an image");
                        return;
                    }


                    // check if the pictures folder contains the new file
                    var newImageFile = Path.ChangeExtension(e.FullPath, JPEG_EXTENSION);
                    var newImageFileName = Path.GetFileName(newImageFile);

                    var existingWallpapers = Directory.GetFiles(WALLPAPER_FOLDER);

                    if (existingWallpapers == null || !existingWallpapers.Any())
                    {
                        File.Copy(e.FullPath, Path.Combine(WALLPAPER_FOLDER, newImageFileName));
                    }
                    else
                    {
                        if (!File.Exists(Path.Combine(WALLPAPER_FOLDER, newImageFileName)))
                        {
                            File.Copy(e.FullPath, Path.Combine(WALLPAPER_FOLDER, newImageFileName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError($"There was an error creating the file: {ex}");
                return;
            }
            
        }

        private bool CheckIfUserHasReadWriteAccess(params string[] paths)
        {
            var hasAccess = true;
            foreach(var path in paths)
            {
                try
                {
                    var newFile = @$"{path}\test.txt";
                    File.WriteAllBytes(newFile, new byte[0]);

                    // user has write access, delete the test one
                    File.Delete(newFile);
                    continue;
                }
                catch (Exception ex)
                {
                    hasAccess = false;
                    this.logger.LogError($"User doesnt have write access to directory: {path}");
                }
            }

            return hasAccess;
        }
    }
}
