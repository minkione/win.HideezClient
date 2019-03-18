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
    class SettingsManager<T> : ISettingsManager<T> where T : BaseSettings, new()
    {
        private string settingsFilePath = string.Empty;
        private T settings;

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
        public T Settings
        {
            get
            {
                if (settings == null)
                    return default(T);
                else
                    return (T)settings.Clone();
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
        /// <returns>Returns loaded program settings</returns>
        public Task<T> GetSettingsAsync()
        {
            if (Settings == null)
                return LoadSettingsAsync();
            else
                return Task.FromResult(Settings);
        }

        /// <summary>
        /// Asynchronously load program settings from file
        /// </summary>
        /// <returns>Returns program settings loaded from file</returns>
        public Task<T> LoadSettingsAsync()
        {
            return Task.Run(() => { return LoadSettings(); });
        }

        /// <summary>
        /// Save program options into file. 
        /// <seealso cref="LoadSettingsAsync(string)"/> is called automatically if save is successful 
        /// </summary>
        /// <param name="settings">Settings that will be saved</param>
        /// <returns>Returns saved settings. Throws exception if save failed</returns>
        public T SaveSettings(T settings)
        {
            var directory = Path.GetDirectoryName(SettingsFilePath);

            FileSerializer.Serialize(SettingsFilePath, settings);

            // Settings reload will automatically update cache and notify observers
            LoadSettings();

            return settings;
        }

        /// <summary>
        /// Synchronously load program settings from file
        /// </summary>
        /// <returns>Returns program settings loaded from file</returns>
        private T LoadSettings()
        {
            var loadedModel = FileSerializer.Deserialize<T>(SettingsFilePath);

            // Should automatically notify all observers that cached settings were changed
            Settings = loadedModel ?? new T();

            return Settings;
        }
    }
}
