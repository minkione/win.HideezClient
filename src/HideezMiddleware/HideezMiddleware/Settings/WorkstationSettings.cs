using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Proximity;
using System;

namespace HideezMiddleware.Settings
{
    public class WorkstationSettings : BaseSettings
    {
        public WorkstationSettings()
        {
            SettingsVersion = new Version(1, 0);
            LockProximity = SdkConfig.DefaultLockProximity;
            UnlockProximity = SdkConfig.DefaultUnlockProximity;
            LockTimeout = SdkConfig.DefaultLockTimeout;
        }

        public WorkstationSettings(WorkstationSettings copy)
        {
            if (copy == null)
                return;

            SettingsVersion = (Version)copy.SettingsVersion.Clone();

            LockProximity = copy.LockProximity;
            UnlockProximity = copy.UnlockProximity;
            LockTimeout = copy.LockTimeout;
        }

        [Setting]
        public Version SettingsVersion { get; }

        [Setting]
        public int LockProximity { get; set; }

        [Setting]
        public int UnlockProximity { get; set; } 

        [Setting]
        public int LockTimeout { get; set; }

        public ProximityMonitorSettings GetProximityMonitorSettings()
        {
            return new ProximityMonitorSettings(LockProximity, UnlockProximity, LockTimeout);
        }

        public override object Clone()
        {
            return new WorkstationSettings(this);
        }
    }
}
