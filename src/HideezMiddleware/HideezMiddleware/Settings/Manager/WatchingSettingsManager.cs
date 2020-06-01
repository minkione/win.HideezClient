using System.IO;

namespace HideezMiddleware.Settings
{
    // Watches for changes in settings file and automatically reloads settings when file is changed
    public class WatchingSettingsManager<T> : SettingsManager<T> where T : BaseSettings, new()
    {
        readonly FileSystemWatcher _watcher;

        public WatchingSettingsManager(string settingsFilePath, IFileSerializer fileSerializer) 
            : base(settingsFilePath, fileSerializer)
        {
            _watcher = new FileSystemWatcher();
            _watcher.Path = Path.GetDirectoryName(settingsFilePath);
            _watcher.Filter = Path.GetFileName(settingsFilePath);
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Attributes | NotifyFilters.CreationTime;

            _watcher.Changed += Watcher_OnChanged;
            _watcher.Deleted += Watcher_OnChanged;
            _watcher.Renamed += Watcher_OnChanged;
        }

        public bool AutoReloadOnFileChanges
        {
            get
            {
                return _watcher.EnableRaisingEvents;
            }
            set
            {
                _watcher.EnableRaisingEvents = value;
            }
        }

        async void Watcher_OnChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                AutoReloadOnFileChanges = false;

                if (e.ChangeType == WatcherChangeTypes.Deleted || e.ChangeType == WatcherChangeTypes.Renamed)
                    InitializeFileStruct();

                await LoadSettingsAsync();
            }
            finally
            {
                // Handles raise of multiple events but may cause bugs if AutoReloadOnFileChanges is changed during loading
                AutoReloadOnFileChanges = true;
            }
        }
    }
}
