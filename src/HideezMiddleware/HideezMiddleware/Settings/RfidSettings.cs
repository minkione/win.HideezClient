using System;

namespace HideezMiddleware.Settings
{
    [Serializable]
    public class RfidSettings : BaseSettings
    {
        public RfidSettings()
        {
            SettingsVersion = new Version(1, 0);
            IsRfidEnabled = false;
        }

        public RfidSettings(RfidSettings copy)
        {
            if (copy == null)
                return;

            SettingsVersion = (Version)copy.SettingsVersion.Clone();

            IsRfidEnabled = copy.IsRfidEnabled;
        }

        [Setting]
        public Version SettingsVersion { get; }

        [Setting]
        public bool IsRfidEnabled { get; set; }

        public override object Clone()
        {
            return new RfidSettings(this);
        }
    }
}
