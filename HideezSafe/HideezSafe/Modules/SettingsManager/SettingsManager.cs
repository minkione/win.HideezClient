using System;
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
        private string settingsFilePath = string.Empty;
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

        /// <summary>
        /// Path to the settings file
        /// <note type="caution">
        /// Changing path value does not automatically update settings cache
        /// </note>
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if assigned path refers to the existing directory instead of file</exception>
        public string SettingsFilePath
        {
            get
            {
                return settingsFilePath;
            }
            set
            {
                if (settingsFilePath != value)
                {
                    if (string.IsNullOrWhiteSpace(value))
                        throw new ArgumentNullException("Settings file path cannot be null or empty");

                    Path.GetFullPath(value); // Will throw exceptions if path is formatted incorrectly

                    if (Directory.Exists(value))
                        throw new ArgumentException("Settings file path cannot refer to the existing directory");

                    if (string.IsNullOrWhiteSpace(Path.GetFileName(value)))
                        throw new ArgumentException("Settings file path must include a filename");

                    settingsFilePath = value;
                }
            }
        }

        /// <summary>
        /// Deep copy of program settings cache
        /// </summary>
        public Settings Settings
        {
            get
            {
                if (settings == null)
                    return null;
                else
                    return new Settings(settings);
            }
            private set
            {
                if (settings != value)
                {
                    settings = value;
                    // Todo: Notify all observers through Messanger that settings were changed
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
            if (Settings == null)
                return LoadSettingsAsync();
            else
                return Task.FromResult(Settings);
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

            // Settings reload will automatically update cache and notify observers
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
