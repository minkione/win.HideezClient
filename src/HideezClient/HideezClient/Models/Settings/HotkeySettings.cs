using HideezMiddleware.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HideezClient.Models.Settings
{
    [Serializable]
    public class HotkeySettings : BaseSettings
    {
        /// <summary>
        /// Initializes new instance of <see cref="HotkeySettings"/> with default values
        /// </summary>
        public HotkeySettings()
        {
            SettingsVersion = new Version(1, 2, 0);
            Hotkeys = new List<Hotkey>()
            {
                new Hotkey(1, true, UserAction.InputLogin, "Control + Alt + L"),
                new Hotkey(2, true, UserAction.InputPassword, "Control + Alt + P"),
                new Hotkey(3, true, UserAction.InputOtp, "Control + Alt + O"),
                new Hotkey(4, true, UserAction.AddAccount, "Control + Alt + A"),
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
            Hotkeys = new List<Hotkey>(copy.Hotkeys.Select(h => h.DeepCopy()));
        }


        [Setting]
        public Version SettingsVersion { get; }

        [Setting]
        public List<Hotkey> Hotkeys { get; set; }

        public override object Clone()
        {
            return new HotkeySettings(this);
        }
    }
}
