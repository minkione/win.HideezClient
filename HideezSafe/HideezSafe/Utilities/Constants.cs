using System;
using System.IO;

namespace HideezSafe.Utilities
{
    static class Constants
    {
        public static string DefaultSettingsFolderPath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
            "Hideez", 
            "Safe", 
            "v3", 
            "Settings");

        public static string DefaultSettingsFileName { get; } = "usersettings.xml";
    }
}
