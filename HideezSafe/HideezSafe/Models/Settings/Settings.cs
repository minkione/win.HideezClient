using System;
using System.Xml.Serialization;

namespace HideezSafe.Models.Settings
{
    [Serializable]
    [XmlRoot(ElementName = "Settings", IsNullable = false)]
    public class Settings : IEquatable<Settings>
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

        public override bool Equals(object obj)
        {
            if (!(obj is Settings))
                return false;

            var other = (Settings)obj;

            return Equals(other);
        }

        public bool Equals(Settings other)
        {
            if (other == null)
                return false;

            return (FirstLaunch == other.FirstLaunch) &&
                (LaunchOnStartup == other.LaunchOnStartup) &&
                (SelectedLanguage == other.SelectedLanguage);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 43;
                int inc = 53;

                hash = hash * inc + FirstLaunch.GetHashCode();
                hash = hash * inc + LaunchOnStartup.GetHashCode();
                hash = hash * inc + SelectedLanguage.GetHashCode();
                return hash;
            }
        }
    }
}
