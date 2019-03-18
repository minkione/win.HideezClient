using System.Threading.Tasks;
using HideezSafe.Models.Settings;
using HideezSafe.Modules.FileSerializer;

namespace HideezSafe.Modules.SettingsManager
{
    interface ISettingsManager
    {
        ApplicationSettings Settings { get; }

        string SettingsFilePath { get; set; }

        Task<ApplicationSettings> GetSettingsAsync();

        Task<ApplicationSettings> LoadSettingsAsync();

        ApplicationSettings SaveSettings(ApplicationSettings settings);
    }
}