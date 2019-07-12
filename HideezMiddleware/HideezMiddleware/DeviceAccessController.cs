using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using HideezMiddleware.Settings;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class DeviceAccessController
    {
        readonly ILogger _log = LogManager.GetCurrentClassLogger();
        readonly SettingsManager<UnlockerSettings> _settingsManager;
        readonly BleDeviceManager _bleDeviceManager;

        UnlockerSettings _unlockerSettings;

        public DeviceAccessController(SettingsManager<UnlockerSettings> settingsManager, BleDeviceManager bleDeviceManager)
        {
            _settingsManager = settingsManager;
            _bleDeviceManager = bleDeviceManager;

            _settingsManager.SettingsChanged += SettingsManager_SettingsChanged;
        }

        void SettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<UnlockerSettings> e)
        {
            _unlockerSettings = e.NewSettings;
            Task.Run(DisconnectNotApprovedDevices);
        }

        async Task DisconnectNotApprovedDevices()
        {
            try
            {
                // Select devices with MAC that is not present in UnlockerSettingsInfo
                var missingDevices = _bleDeviceManager.Devices.Where(d => !_unlockerSettings.DeviceUnlockerSettings.Any(s => s.Mac == d.Mac));

                foreach (var device in missingDevices)
                    await RemoveDevice(device);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
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
                _log.Error(ex);
            }
        }


        public bool IsProximityAllowed(string mac)
        {
            return _unlockerSettings.DeviceUnlockerSettings.Any(s => s.Mac == mac && s.AllowProximity);
        }

        public bool IsRfidAllowed(string mac)
        {
            return _unlockerSettings.DeviceUnlockerSettings.Any(s => s.Mac == mac && s.AllowRfid);
        }

        public bool IsBleTapAllowed(string mac)
        {
            return _unlockerSettings.DeviceUnlockerSettings.Any(s => s.Mac == mac && s.AllowBleTap);
        }
    }
}
