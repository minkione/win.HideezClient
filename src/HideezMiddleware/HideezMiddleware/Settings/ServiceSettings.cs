using System;

namespace HideezMiddleware.Settings
{
    [Serializable]
    public class ServiceSettings : BaseSettings
    {
        public ServiceSettings()
        {
            SettingsVersion = new Version(1, 0);
            ReadDeviceLog = false;
            ClearDeviceLogsAfterRead = false;
            ClearDeviceLogsAfterDays = 3;
            EnableSoftwareVaultUnlock = false;
        }

        public ServiceSettings(ServiceSettings copy)
        {
            if (copy == null)
                return;

            SettingsVersion = (Version)copy.SettingsVersion.Clone();

            ReadDeviceLog = copy.ReadDeviceLog;
            ClearDeviceLogsAfterRead = copy.ClearDeviceLogsAfterRead;
            ClearDeviceLogsAfterDays = copy.ClearDeviceLogsAfterDays;
            EnableSoftwareVaultUnlock = copy.EnableSoftwareVaultUnlock;
        }

        [Setting]
        public Version SettingsVersion { get; }

        [Setting]
        public bool ReadDeviceLog { get; set; }

        [Setting]
        public bool ClearDeviceLogsAfterRead { get; set; }

        [Setting]
        public int ClearDeviceLogsAfterDays { get; set; }

        [Setting]
        public bool EnableSoftwareVaultUnlock { get; set; }

        public override object Clone()
        {
            return new ServiceSettings(this);
        }
    }
}
