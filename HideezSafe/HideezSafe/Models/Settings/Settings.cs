using System;
using System.Xml.Serialization;

namespace HideezSafe.Models.Settings
{
    [Serializable]
    [XmlRoot(ElementName = "Settings", IsNullable = false)]
    public class Settings
    {
        /// <summary>
        /// Initializes new instance of <see cref="Settings"/> with default values
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
        /// <param name="copy">Intance to copy from</param>
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
        public bool FirstLaunch { get; set; }

        [XmlElement(ElementName = "LaunchOnStartup")]
        public bool LaunchOnStartup { get; set; }

        [XmlElement(ElementName = "SelectedLanguage")]
        public string SelectedLanguage { get; set; }
    }
}
