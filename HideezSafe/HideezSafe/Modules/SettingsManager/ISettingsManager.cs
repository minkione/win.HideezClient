using HideezSafe.Models.Settings;
using System.Threading.Tasks;

namespace HideezSafe.Modules.SettingsManager
{
    interface ISettingsManager
    {
        /// <summary>
        /// Program settings cache
        /// </summary>
        ISettings Settings { get; }

        /// <summary>
        /// Get settings from cache. If not available, load from file
        /// </summary>
        /// <param name="settingsFilePath">Path to settings file</param>
        /// <returns>Returns loaded program settings</returns>
        Task<ISettings> GetSettingsAsync(string settingsFilePath);

        /// <summary>
        /// Asynchronously load program settings from file
        /// </summary>
        /// <param name="settingsFilePath">Path to settings file</param>
        /// <returns>Returns program settings loaded from file</returns>
        Task<ISettings> LoadSettingsAsync(string settingsFilePath);

        /// <summary>
        /// Save program options into file
        /// </summary>
        /// <param name="settingsFileName">Path to settings file</param>
        /// <param name="optionsModel">Settings that will be saved</param>
        /// <returns>Returns true if successfully saved settings into file; Otherwise throws exception</returns>
        bool SaveSettings(string settingsFilePath, ISettings settings);

    }
}
