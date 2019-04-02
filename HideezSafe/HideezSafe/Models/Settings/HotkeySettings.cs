using HideezSafe.Modules.HotkeyManager;
using System;
using System.Collections.Generic;

namespace HideezSafe.Models.Settings
{
    [Serializable]
    public class HotkeySettings : BaseSettings
    {
        /// <summary>
        /// Initializes new instance of <see cref="HotkeySettings"/> with default values
        /// </summary>
        public HotkeySettings()
        {
            SettingsVersion = new Version(1, 0, 0);
            Hotkeys = new Dictionary<UserAction, string>()
            {
                {UserAction.InputLogin, "Control + Alt + L" },
                {UserAction.InputPassword, "Control + Alt + P" },
                {UserAction.InputDefaultPassword, "Control + Alt + D" },
                {UserAction.AddPassword, "Control + Alt + A" },
                {UserAction.InputOtp, "Control + Alt + O" },
            };
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy">Intance to copy from</param>
        public HotkeySettings(HotkeySettings copy)
            :base()
        {
            if (copy == null)
                return;

            SettingsVersion = (Version)copy.SettingsVersion.Clone();
            Hotkeys = new Dictionary<UserAction, string>(copy.Hotkeys.Count, copy.Hotkeys.Comparer);
            foreach (var h in copy.Hotkeys)
            {
                Hotkeys.Add(h.Key, h.Value);
            }
        }


        [Setting]
        public Version SettingsVersion { get; }

        [Setting]
        public Dictionary<UserAction, string> Hotkeys { get; set; }

        public override object Clone()
        {
            return new HotkeySettings(this);
        }
    }
}
