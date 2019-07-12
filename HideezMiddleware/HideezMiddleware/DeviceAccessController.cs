using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using HideezMiddleware.Settings;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    /// <summary>
    /// Monitors change in unlocker settings and automatically disconnects all devices that are no longer authorized for access
    /// </summary>
    public class DeviceAccessController
    {
        readonly ILogger _log = LogManager.GetCurrentClassLogger();
        readonly ISettingsManager<UnlockerSettings> _unlockerSettingsManager;
        readonly BleDeviceManager _bleDeviceManager;

        public DeviceAccessController(ISettingsManager<UnlockerSettings> unlockerSettingsManager, BleDeviceManager bleDeviceManager)
        {
            _unlockerSettingsManager = unlockerSettingsManager;
            _bleDeviceManager = bleDeviceManager;

            _unlockerSettingsManager.SettingsChanged += SettingsManager_SettingsChanged;
        }

        public bool IsEnabled { get; private set; } = false;

        public void Start()
        {
            IsEnabled = true;
        }

        public void Stop()
        {
            IsEnabled = false;
        }

        async void SettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<UnlockerSettings> e)
        {
            if (IsEnabled)
                await DisconnectNotApprovedDevices(e.NewSettings.DeviceUnlockerSettings);
        }

        async Task DisconnectNotApprovedDevices(DeviceUnlockerSettingsInfo[] newDeviceUnlockerSettings)
        {
            try
            {
                var unlockerSettings = await _unlockerSettingsManager.GetSettingsAsync();

                // Select devices with MAC that is not present in UnlockerSettingsInfo
                var missingDevices = _bleDeviceManager.Devices.Where(d => !unlockerSettings.DeviceUnlockerSettings.Any(s => s.Mac.Replace(":", "") == d.Mac));

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

    }
}
