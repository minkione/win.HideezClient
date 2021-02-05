using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity.Interfaces;
using HideezMiddleware.ApplicationModeProvider;
using System;
using System.Linq;

namespace HideezMiddleware.Settings.SettingsProvider
{
    public class DeviceProximitySettingsProvider: Logger, IDeviceProximitySettingsProvider
    {
        readonly ApplicationMode _applicationMode;
        readonly string _deviceId;
        readonly ISettingsManager<UserProximitySettings> _devicesProximitySettingsManager;
        readonly ISettingsManager<ProximitySettings> _enterpriseProximitySettingsManager;

        UserDeviceProximitySettings _userDeviceProximitySettings;
        bool _isExistEnterpriseSettings = false;

        public int LockProximity
        {
            get => _applicationMode == ApplicationMode.Enterprise ? SdkConfig.DefaultLockProximity
                : _userDeviceProximitySettings.LockProximity;
        }

        public int UnlockProximity
        {
            get => _applicationMode == ApplicationMode.Enterprise ? SdkConfig.DefaultUnlockProximity
                : _userDeviceProximitySettings.UnlockProximity;
        }

        public int LockTimeout
        {
            get => _applicationMode == ApplicationMode.Enterprise ? SdkConfig.DefaultLockTimeout
                : SdkConfig.DefaultLockTimeout;
        }

        public int ProximityTimeout
        {
            get => LockTimeout * 2;
        }

        public bool EnabledLockByProximity 
        { 
            get => _applicationMode == ApplicationMode.Enterprise ? true : _userDeviceProximitySettings.EnabledLockByProximity;
        }

        public bool EnabledUnlockByProximity 
        { 
            get => _applicationMode == ApplicationMode.Enterprise ? true : _userDeviceProximitySettings.EnabledUnlockByProximity;
        }

        public bool DisabledDisplayAuto 
        { 
            get => _applicationMode == ApplicationMode.Enterprise ? _isExistEnterpriseSettings : _userDeviceProximitySettings.DisabledDisplayAuto;
        }

        public DeviceProximitySettingsProvider(
            string deviceId,
            ApplicationMode applicationMode,
            ISettingsManager<ProximitySettings> enterpriseProximitySettingsManager,
            ISettingsManager<UserProximitySettings> userDevicesProximitySettingsManager,
            ILog log):
            base(nameof(DeviceProximitySettingsProvider), log)
        {
            _deviceId = deviceId;
            _applicationMode = applicationMode;
            _enterpriseProximitySettingsManager = enterpriseProximitySettingsManager;
            _devicesProximitySettingsManager = userDevicesProximitySettingsManager;

            if (_applicationMode == ApplicationMode.Enterprise)
                _enterpriseProximitySettingsManager.SettingsChanged += WorkstationProximitySettingsManager_SettingsChanged;
            else
                _devicesProximitySettingsManager.SettingsChanged += DevicesProximitySettingsManager_SettingsChanged;

            LoadSettings();
        }

        private void WorkstationProximitySettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<ProximitySettings> e)
        {
            LoadSettings();
        }

        private void DevicesProximitySettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<UserProximitySettings> e)
        {
            LoadSettings();
        }

        void LoadSettings() 
        {
            try
            {
                if (_applicationMode == ApplicationMode.Enterprise)
                {
                    var settings = _enterpriseProximitySettingsManager.Settings;
                    _isExistEnterpriseSettings = settings.DevicesProximity.Any(s=>s.Mac == BleUtils.ConnectionIdToMac(_deviceId));
                }
                else
                {
                    var settings = _devicesProximitySettingsManager.Settings;
                    var newDeviceSettings = settings.GetProximitySettings(_deviceId);
                    if (ValidateSettings(newDeviceSettings.LockProximity, newDeviceSettings.UnlockProximity, SdkConfig.DefaultLockTimeout))
                        _userDeviceProximitySettings = newDeviceSettings;
                }

                WriteLine($"{_deviceId} monitor settings set to lock:{LockProximity}, unlock:{UnlockProximity}, " +
                    $"lock_timeout:{LockTimeout}, proximity_timeout:{ProximityTimeout}");
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        bool ValidateSettings(int lockProximity, int unlockProximity, int lockTimeout)
        {
            if (lockProximity < 0 || lockProximity > 100)
                throw new ArgumentOutOfRangeException("Lock proximity must be in range from 0 to 100");

            if (unlockProximity < 0 || unlockProximity > 100)
                throw new ArgumentOutOfRangeException("Unlock proximity must be in range from 0 to 100");

            if (unlockProximity <= lockProximity)
                throw new ArgumentOutOfRangeException("Unlock proximity must be equal or lower than lock proximity");

            if (lockTimeout < 0)
                throw new ArgumentOutOfRangeException("Lock delay seconds must be more than 0.");

            return true;
        }
    }
}
