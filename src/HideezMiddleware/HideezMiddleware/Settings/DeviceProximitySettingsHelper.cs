using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.Settings
{
    public class DeviceProximitySettingsHelper
    {
        private readonly ISettingsManager<ProximitySettings> proximitySettingsManager;
        private readonly ISettingsManager<DevicePermissionsSettings> devicePermissionsSettingsManager;

        public DeviceProximitySettingsHelper(ISettingsManager<ProximitySettings> proximitySettingsManager, ISettingsManager<DevicePermissionsSettings> devicePermissionsSettingsManager)
        {
            this.proximitySettingsManager = proximitySettingsManager;
            this.devicePermissionsSettingsManager = devicePermissionsSettingsManager;
        }

        public bool GetAllowEditProximity(string mac)
        {
            return true;
        }

        private void UpdateAllowEditProximity(string mac, bool allowEdit)
        {

        }

        public void SaveOrUpdate(IReadOnlyList<DeviceProximitySettingsDto> dtos)
        {
            var settings = proximitySettingsManager.Settings;
            foreach (var dto in dtos)
            {
                Update(settings, dto);
            }
            proximitySettingsManager.SaveSettings(settings);
        }

        public void SetClientProximity(string mac, int lockProximity, int unlockProximity)
        {
            var settings = proximitySettingsManager.Settings;
            var deviceProximity = settings.GetProximitySettings(mac);

            deviceProximity.ClientLockProximity = lockProximity;
            deviceProximity.ClientUnlockProximity = unlockProximity;

            if (GetAllowEditProximity(mac))
            {
                deviceProximity.LockProximity = lockProximity;
                deviceProximity.UnlockProximity = unlockProximity;
            }
            proximitySettingsManager.SaveSettings(settings);
        }


        private void Update(ProximitySettings settings, DeviceProximitySettingsDto dto)
        {
            var deviceProximity = settings.GetProximitySettings(dto.Mac);

            deviceProximity.Mac = dto.Mac;
            deviceProximity.SerialNo = dto.SerialNo;
            deviceProximity.LockTimeout = dto.LockTimeout;

            deviceProximity.ServerLockProximity = dto.LockProximity;
            deviceProximity.ServerUnlockProximity = dto.UnlockProximity;

            if (!GetAllowEditProximity(dto.Mac))
            {
                deviceProximity.LockProximity = dto.LockProximity;
                deviceProximity.UnlockProximity = dto.UnlockProximity;
            }
        }
    }
}
