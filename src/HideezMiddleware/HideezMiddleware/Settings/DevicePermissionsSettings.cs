using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.Settings
{
    [Serializable]
    public class DevicePermissionsSettings : BaseSettings
    {
        public DevicePermissionsSettings()
        {
            SettingsVersion = new Version(1, 0, 0);
        }

        public DevicePermissionsSettings(DevicePermissionsSettings copy)
        {
            if (copy == null)
                return;

            SettingsVersion = (Version)copy.SettingsVersion.Clone();
            SerialNo = copy.SerialNo;
            Mac = copy.Mac;
            AllowEditProximitySettings = copy.AllowEditProximitySettings;
        }

        [Setting]
        public Version SettingsVersion { get; }
        [Setting]
        public string SerialNo { get; set; }
        [Setting]
        public string Mac { get; set; }
        [Setting]
        public bool AllowEditProximitySettings { get; set; }

        public override object Clone()
        {
            return new DevicePermissionsSettings(this);
        }
    }
}
