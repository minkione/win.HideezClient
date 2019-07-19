using Hideez.SDK.Communication;
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
        readonly IWorkstationLocker _workstationLocker;

        public DeviceAccessController(ISettingsManager<UnlockerSettings> unlockerSettingsManager, BleDeviceManager bleDeviceManager, IWorkstationLocker workstationLocker)
        {
            _unlockerSettingsManager = unlockerSettingsManager;
            _bleDeviceManager = bleDeviceManager;
            _workstationLocker = workstationLocker;
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
                var missingDevices = _bleDeviceManager.Devices.Where(d => !unlockerSettings.DeviceUnlockerSettings.Any(s => s.Mac == d.Mac));

                var isConnectedDevices = missingDevices.Where(d => d.IsConnected).ToArray();
                if (isConnectedDevices.Any())
                {
                    _log.Info($"Locking workstation: some devices are no longer authorized to work with this workstation.");
                    SessionSwitchManager.SetEventSubject(SessionSwitchSubject.AccessCancelled, missingDevices.First().SerialNo);
                    _workstationLocker.LockWorkstation();
                }

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
                _log.Info($"{device.Id} is no longer authorized on this workstation. Disconnecting.");
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
