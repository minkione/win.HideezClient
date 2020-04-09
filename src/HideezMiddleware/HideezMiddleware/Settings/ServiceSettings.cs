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
            ClearDeviceLogsAfterDays = 30;
        }

        public ServiceSettings(ServiceSettings copy)
        {
            if (copy == null)
                return;

            SettingsVersion = (Version)copy.SettingsVersion.Clone();

            ReadDeviceLog = copy.ReadDeviceLog;
            ClearDeviceLogsAfterRead = copy.ClearDeviceLogsAfterRead;
            ClearDeviceLogsAfterDays = copy.ClearDeviceLogsAfterDays;
        }

        [Setting]
        public Version SettingsVersion { get; }

        [Setting]
        public bool ReadDeviceLog { get; set; }

        [Setting]
        public bool ClearDeviceLogsAfterRead { get; set; }

        [Setting]
        public int ClearDeviceLogsAfterDays { get; set; }

        public override object Clone()
        {
            return new ServiceSettings(this);
        }
    }
}
