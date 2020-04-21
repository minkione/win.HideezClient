using System;
using System.Threading.Tasks;

namespace HideezMiddleware.Settings.Manager
{
    /// <summary>
    /// Does not actually load or save settings and always provides the default settings
    /// </summary>
    public class VirtualSettingsManager<T> : ISettingsManager<T> where T : BaseSettings, new()
    {
        T _settings = new T();

        public T Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                if (_settings != value)
                {
                    var oldValue = _settings;
                    _settings = value;
                    SettingsChanged?.Invoke(this, new SettingsChangedEventArgs<T>(oldValue, _settings));
                }
            }
        }

        public string SettingsFilePath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public event EventHandler<SettingsChangedEventArgs<T>> SettingsChanged;

        public Task<T> GetSettingsAsync()
        {
            return Task.FromResult(Settings);
        }

        public void InitializeFileStruct()
        {
        }

        public Task<T> LoadSettingsAsync()
        {
            return Task.FromResult(Settings);
        }

        public T SaveSettings(T settings)
        {
            Settings = settings;
            return Settings;
        }
    }
}
