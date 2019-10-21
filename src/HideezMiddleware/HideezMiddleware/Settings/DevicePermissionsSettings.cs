using Hideez.SDK.Communication.HES.Client;
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
            DevicesPermissions = Array.Empty<DevicePermissions>();
        }

        public DevicePermissionsSettings(DevicePermissionsSettings copy)
        {
            if (copy == null)
                return;

            SettingsVersion = (Version)copy.SettingsVersion.Clone();

            var devicesPermissionsSettings = new List<DevicePermissions>(copy.DevicesPermissions.Length);

            foreach (var settings in copy.DevicesPermissions)
            {
                devicesPermissionsSettings.Add(new DevicePermissions
                {
                    SerialNo = settings.SerialNo,
                    Mac = settings.Mac,
                    AllowEditProximitySettings = settings.AllowEditProximitySettings,
                });
            }

            DevicesPermissions = devicesPermissionsSettings.ToArray();

        }

        public DevicePermissions[] DevicesPermissions { get; set; }

        [Setting]
        public Version SettingsVersion { get; }

        public override object Clone()
        {
            return new DevicePermissionsSettings(this);
        }

        public DevicePermissions GetPermissions(string mac)
        {
            var permissinons = DevicesPermissions.FirstOrDefault(p => p.Mac == mac) ?? new DevicePermissions { Mac = mac, AllowEditProximitySettings = true, };

            return permissinons;
        }
    }
}
