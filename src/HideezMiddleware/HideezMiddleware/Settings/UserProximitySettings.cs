using Hideez.SDK.Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.Settings
{
    public class UserProximitySettings: BaseSettings
    {
        [Setting]
        public Version SettingsVersion { get; }

        [Setting]
        public UserDeviceProximitySettings[] DevicesProximity { get; set; }

        /// <summary>
        /// Initializes new instance of <see cref="ProximitySettings"/> with default values
        /// </summary>
        public UserProximitySettings()
        {
            SettingsVersion = new Version(1, 0);
            DevicesProximity = Array.Empty<UserDeviceProximitySettings>();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy">Intance to copy from</param>
        public UserProximitySettings(UserProximitySettings copy)
        {
            if (copy == null)
                return;

            SettingsVersion = (Version)copy.SettingsVersion.Clone();

            var deviceUnlockerSettings = new List<UserDeviceProximitySettings>(copy.DevicesProximity.Length);

            foreach (var settings in copy.DevicesProximity)
            {
                deviceUnlockerSettings.Add(new UserDeviceProximitySettings
                {
                    Id = settings.Id,
                    LockProximity = settings.LockProximity,
                    UnlockProximity = settings.UnlockProximity,
                    EnabledLockByProximity = settings.EnabledLockByProximity,
                    EnabledUnlockByProximity = settings.EnabledUnlockByProximity,
                    DisabledDisplayAuto = settings.DisabledDisplayAuto
                });
            }

            DevicesProximity = deviceUnlockerSettings.ToArray();
        }

        /// <summary>
        /// Return proximity settings for device, if not found settings return default settings
        /// </summary>
        public UserDeviceProximitySettings GetProximitySettings(IDevice device)
        {
            return GetProximitySettings(device.Id);
        }

        /// <summary>
        /// Return proximity settings for device, if not found settings return default settings
        /// </summary>
        public UserDeviceProximitySettings GetProximitySettings(string id)
        {
            var deviceProximity = DevicesProximity.FirstOrDefault(s => s.Id == id);
            if (deviceProximity == null)
            {
                deviceProximity = UserDeviceProximitySettings.DefaultSettings;
                deviceProximity.Id = id;
            }
            return deviceProximity;
        }

        public override object Clone()
        {
            return new UserProximitySettings(this);
        }
    }
}
