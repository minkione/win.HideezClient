using System;
using System.Threading.Tasks;

namespace HideezMiddleware.Settings
{
    public class SettingsChangedEventArgs<T> where T : BaseSettings, new()
    {
        public SettingsChangedEventArgs(T oldSettings, T newSettings)
        {
            OldSettings = oldSettings;
            NewSettings = newSettings;
        }

        public T OldSettings { get; }
        public T NewSettings { get; }
    }

    public interface ISettingsManager<T> where T : BaseSettings, new()
    {
        event EventHandler<SettingsChangedEventArgs<T>> SettingsChanged;

        T Settings { get; }

        string SettingsFilePath { get; set; }

        Task<T> GetSettingsAsync();

        Task<T> LoadSettingsAsync();

        T SaveSettings(T settings);

        T LoadSettings();

        void InitializeFileStruct();
    }
}
