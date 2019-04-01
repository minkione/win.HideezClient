using System.Threading.Tasks;
using HideezSafe.Models.Settings;

namespace HideezSafe.Modules.SettingsManager
{
    interface ISettingsManager<T> where T : BaseSettings, new()
    {
        T Settings { get; }

        string SettingsFilePath { get; set; }

        Task<T> GetSettingsAsync();

        Task<T> LoadSettingsAsync();

        T SaveSettings(T settings);
    }
}