using System.Xml.Serialization;

namespace HideezSafe.Models.Settings
{
    interface ISettings
    {
        [XmlElement(ElementName = "FirstLaunch")]
        bool FirstLaunch { get; set; }

        [XmlElement(ElementName = "LaunchOnStartup")]
        bool LaunchOnStartup { get; set; }

        [XmlElement(ElementName = "SelectedLanguage")]
        string SelectedLanguage { get; set; }
    }
}
