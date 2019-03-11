using System.Threading.Tasks;
using HideezSafe.Models.Settings;
using HideezSafe.Modules.FileSerializer;

namespace HideezSafe.Modules.SettingsManager
{
    interface ISettingsManager
    {
        Settings Settings { get; }

        string SettingsFilePath { get; set; }

        Task<Settings> GetSettingsAsync();

        Task<Settings> LoadSettingsAsync();

        Settings SaveSettings(Settings settings);
    }
}