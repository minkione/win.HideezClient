using System;
using System.IO;

namespace HideezClient.Utilities
{
    static class Constants
    {
        public static string DefaultSettingsFolderPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
            "Hideez", 
            "Safe", 
            "v3", 
            "Settings");

        public static string ApplicationSettingsFileName { get; } = "applicationsettings.xml";
        public static string HotkeySettingsFileName { get; } = "hotkeysettings.xml";
    }
}
