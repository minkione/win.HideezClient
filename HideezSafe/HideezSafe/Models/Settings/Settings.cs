using System;
using System.Xml.Serialization;

namespace HideezSafe.Models.Settings
{
    [Serializable]
    [XmlRoot(ElementName = "Settings", IsNullable = false)]
    class Settings : ISettings
    {
        private bool firstLaunch;
        private bool launchOnStartup;
        private string selectedLanguage;

        /// <summary>
        /// Class constructor
        /// Initializes with default values
        /// </summary>
        public Settings()
        {
            FirstLaunch = true;
            LaunchOnStartup = false;
            SelectedLanguage = "en-us";
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public Settings(Settings copy)
            :this()
        {
            if (copy == null)
                return;

            FirstLaunch = copy.FirstLaunch;
            LaunchOnStartup = copy.LaunchOnStartup;
            SelectedLanguage = copy.SelectedLanguage;
        }

        [XmlElement(ElementName = "FirstLaunch")]
        public bool FirstLaunch
        {
            get
            {
                return firstLaunch;
            }
            set
            {
                if (firstLaunch != value)
                {
                    firstLaunch = value;
                }
            }
        }

        [XmlElement(ElementName = "LaunchOnStartup")]
        public bool LaunchOnStartup
        {
            get
            {
                return launchOnStartup;
            }
            set
            {
                if (launchOnStartup != value)
                {
                    launchOnStartup = value;
                }
            }
        }

        [XmlElement(ElementName = "SelectedLanguage")]
        public string SelectedLanguage
        {
            get
            {
                return selectedLanguage;
            }
            set
            {
                if (selectedLanguage != value)
                {
                    selectedLanguage = value;
                }
            }
        }
    }
}
