using HideezMiddleware.Settings;
using System;

namespace HideezClient.Models.Settings
{
    [Serializable]
    public class ApplicationSettings : BaseSettings
    {
        /// <summary>
        /// Initializes new instance of <see cref="ApplicationSettings"/> with default values
        /// </summary>
        public ApplicationSettings()
        {
            SettingsVersion = new Version(1, 0, 0);
            IsFirstLaunch = true;
            LaunchApplicationOnStartup = false;
            SelectedUiLanguage = "en-us";
            AddEnterAfterInput = false;
            LimitPasswordEntry = false;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy">Intance to copy from</param>
        public ApplicationSettings(ApplicationSettings copy)
            :this()
        {
            if (copy == null)
                return;

            SettingsVersion = (Version)copy.SettingsVersion.Clone();
            IsFirstLaunch = copy.IsFirstLaunch;
            LaunchApplicationOnStartup = copy.LaunchApplicationOnStartup;
            SelectedUiLanguage = copy.SelectedUiLanguage;
            AddEnterAfterInput = copy.AddEnterAfterInput;
            LimitPasswordEntry = copy.LimitPasswordEntry;
        }

        [Setting]
        public Version SettingsVersion { get; }

        [Setting]
        public bool IsFirstLaunch { get; set; }
        
        [Setting]
        public bool LaunchApplicationOnStartup { get; set; }

        [Setting]
        public string SelectedUiLanguage { get; set; }

        [Setting]
        public bool AddEnterAfterInput { get; set; }

        [Setting]
        public bool LimitPasswordEntry { get; set; }

        public override object Clone()
        {
            return new ApplicationSettings(this);
        }
    }
}
