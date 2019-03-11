using System.IO;
using System.Threading.Tasks;
using HideezSafe.Models.Settings;
using HideezSafe.Modules.FileSerializer;
using HideezSafe.Utilities;
using Unity;

namespace HideezSafe.Modules.SettingsManager
{
    /// <summary>
    /// This class keeps track of program settings
    /// Settings should loaded on the application start and are kept in cache until updated
    /// See <seealso cref="ISettings"/> and <seealso cref="Settings"/> for information about available settings
    /// </summary>
    class SettingsManager : ISettingsManager
    {
        private Settings settings = null;

        /// <summary>
        /// Initializes a new instance of <see cref="SettingsManager"/> class
        /// </summary>
        public SettingsManager()
        {
            // Initialize with default settings file path specified in constants
            SettingsFilePath = Path.Combine(Constants.DefaultSettingsFolderPath, Constants.DefaultSettingsFileName);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SettingsManager"/> class
        /// </summary>
        /// <param name="settingsFilePath">Path to the settings file</param>
        public SettingsManager(string settingsFilePath)
        {
            SettingsFilePath = settingsFilePath;
        }

        /// <summary>
        /// Injection property of class responsible for settings serialization and deserialization
        /// </summary>
        [Dependency]
        public IFileSerializer FileSerializer { get; set; }

        // Todo: Check that a path ends with filename
        /// <summary>
        /// Path to the settings file. 
        /// <note type="caution">
        /// Changing path value does not automatically update settings cache
        /// </note>
        /// </summary>
        public string SettingsFilePath { get; set; } = string.Empty;

        /// <summary>
        /// Program settings cache
        /// </summary>
        public Settings Settings
        {
            get
            {
                return settings;
            }
            private set
            {
                if (settings != value)
                {
                    settings = value;
                    // Todo: Notify through Messanger that settings were changed
                }
            }
        }

        /// <summary>
        /// Get settings from cache. If not available, load from file
        /// </summary>
        /// <param name="settingsFilePath">Path to settings file</param>
        /// <returns>Returns loaded program settings</returns>
        public Task<Settings> GetSettingsAsync()
        {
            return Task.FromResult(Settings) ?? LoadSettingsAsync();
        }

        /// <summary>
        /// Asynchronously load program settings from file
        /// </summary>
        /// <param name="settingsFilePath">Path to settings file</param>
        /// <returns>Returns program settings loaded from file</returns>
        public Task<Settings> LoadSettingsAsync()
        {
            return Task.Run(() => { return LoadSettings(); });
        }

        /// <summary>
        /// Save program options into file. 
        /// <seealso cref="LoadSettingsAsync(string)"/> is called automatically if save is successful 
        /// </summary>
        /// <param name="settingsFileName">Path to settings file</param>
        /// <param name="optionsModel">Settings that will be saved</param>
        /// <returns>Returns saved settings. Throws exception if save failed</returns>
        public Settings SaveSettings(Settings settings)
        {
            var directory = Path.GetDirectoryName(SettingsFilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            FileSerializer.Serialize(SettingsFilePath, settings);

            // Will automatically update cache and notify observers
            LoadSettings();

            return settings;
        }

        /// <summary>
        /// Synchronously load program settings from file
        /// </summary>
        /// <param name="settingsFileName">Path to settings file</param>
        /// <returns>Returns program settings loaded from file</returns>
        private Settings LoadSettings()
        {
            var loadedModel = FileSerializer.Deserialize<Settings>(SettingsFilePath);

            // Should automatically notify all observers that cached settings were changed
            Settings = loadedModel ?? new Settings();

            return Settings;
        }
    }
}
