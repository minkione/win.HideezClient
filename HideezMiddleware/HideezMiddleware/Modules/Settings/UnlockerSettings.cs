using Hideez.SDK.Communication.HES.Client;
using HideezMiddleware.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.Settings
{
    [Serializable]
    public class UnlockerSettings : BaseSettings
    {
        /// <summary>
        /// Initializes new instance of <see cref="UnlockerSettings"/> with default values
        /// </summary>
        public UnlockerSettings()
        {
            SettingsVersion = new Version(1, 0, 0);
            LockProximity = 30;
            UnlockProximity = 50;
            LockTimeoutSeconds = 3;

            DeviceUnlockerSettings = new DeviceUnlockerSettingsInfo[] { };
        }

        public UnlockerSettings(UnlockerSettingsInfo unlockerSettingsInfo)
        {
            SettingsVersion = new Version(1, 0, 0);

            LockProximity = unlockerSettingsInfo.LockProximity;
            UnlockProximity = unlockerSettingsInfo.UnlockProximity;
            LockTimeoutSeconds = unlockerSettingsInfo.LockTimeoutSeconds;
            DeviceUnlockerSettings = unlockerSettingsInfo.DeviceUnlockerSettings;

            for (int i = 0; i < DeviceUnlockerSettings.Length; i++)
            {
                DeviceUnlockerSettings[i].Mac = DeviceUnlockerSettings[i].Mac.Replace(":", "");
            }
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy">Intance to copy from</param>
        public UnlockerSettings(UnlockerSettings copy)
        {
            if (copy == null)
                return;

            SettingsVersion = (Version)copy.SettingsVersion.Clone();
            LockProximity = copy.LockProximity;
            UnlockProximity = copy.UnlockProximity;
            LockTimeoutSeconds = copy.LockTimeoutSeconds;

            List<DeviceUnlockerSettingsInfo> deviceUnlockerSettings = new List<DeviceUnlockerSettingsInfo>();

            foreach (var settings in copy.DeviceUnlockerSettings)
            {
                deviceUnlockerSettings.Add(new DeviceUnlockerSettingsInfo
                {
                    AllowBleTap = settings.AllowBleTap,
                    AllowProximity = settings.AllowProximity,
                    AllowRfid = settings.AllowRfid,
                    RequirePin = settings.RequirePin,
                    SerialNo = settings.SerialNo,
                    Mac = settings.Mac.Replace(":", ""),
                });
            }

            DeviceUnlockerSettings = deviceUnlockerSettings.ToArray();
        }

        [Setting]
        public Version SettingsVersion { get; }

        [Setting]
        public int LockProximity { get; set; }

        [Setting]
        public int UnlockProximity { get; set; }

        [Setting]
        public int LockTimeoutSeconds { get; set; }

        [Setting]
        public DeviceUnlockerSettingsInfo[] DeviceUnlockerSettings { get; set; }

        public override object Clone()
        {
            return new UnlockerSettings(this);
        }
    }
}
