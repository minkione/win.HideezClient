using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class DeviceAccessController
    {
        readonly SettingsManager _settingsManager;
        readonly BleDeviceManager _bleDeviceManager;

        UnlockerSettingsInfo _unlockerSettingsInfo;

        public DeviceAccessController(SettingsManager settingsManager, BleDeviceManager bleDeviceManager)
        {
            _settingsManager = settingsManager;
            _bleDeviceManager = bleDeviceManager;

            _settingsManager.SettingsUpdated += SettingsManager_SettingsUpdated;
        }

        void SettingsManager_SettingsUpdated(object sender, Hideez.SDK.Communication.UnlockerSettingsInfoEventArgs e)
        {
            _unlockerSettingsInfo = e.NewSettings;
            Task.Run(DisconnectNotApprovedDevices);
        }

        async Task DisconnectNotApprovedDevices()
        {
            try
            {
                // Select devices with MAC that is not present in UnlockerSettingsInfo
                var missingDevices = _bleDeviceManager.Devices.Where(d => !_unlockerSettingsInfo.DeviceUnlockerSettings.Any(s => s.Mac == d.Mac));

                foreach (var device in missingDevices)
                    await RemoveDevice(device);
            }
            catch (Exception ex)
            {
                // todo: Add NLog
            }
        }

        async Task RemoveDevice(IDevice device)
        {
            try
            {
                await device.Disconnect();
                await _bleDeviceManager.Remove(device);
            }
            catch (Exception ex)
            {
                // todo: Add NLog
            }
        }

        public bool IsProximityAllowed(string mac)
        {
            return _unlockerSettingsInfo.DeviceUnlockerSettings.Any(s => s.Mac == mac && s.AllowProximity);
        }

        public bool IsRfidAllowed(string mac)
        {
            return _unlockerSettingsInfo.DeviceUnlockerSettings.Any(s => s.Mac == mac && s.AllowRfid);
        }

        public bool IsBleTapAllowed(string mac)
        {
            return _unlockerSettingsInfo.DeviceUnlockerSettings.Any(s => s.Mac == mac && s.AllowBleTap);
        }
    }
}
