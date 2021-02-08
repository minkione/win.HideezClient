using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity.Interfaces;
using System;

namespace HideezMiddleware.Settings.SettingsProvider
{
    public class UserProximitySettingsProvider: Logger, IDeviceProximitySettingsProvider
    {
        readonly ISettingsManager<UserProximitySettings> _devicesProximitySettingsManager;

        UserProximitySettings _userProximitySettings;

        public UserProximitySettingsProvider(
            ISettingsManager<UserProximitySettings> userDevicesProximitySettingsManager,
            ILog log):
            base(nameof(UserProximitySettingsProvider), log)
        {
            _devicesProximitySettingsManager = userDevicesProximitySettingsManager;

            _devicesProximitySettingsManager.SettingsChanged += DevicesProximitySettingsManager_SettingsChanged;

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
                _userProximitySettings = _devicesProximitySettingsManager.Settings;
                foreach (var settings in _userProximitySettings.DevicesProximity)
                    ValidateSettings(settings.Id, settings.LockProximity, settings.UnlockProximity, SdkConfig.DefaultLockTimeout);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        bool ValidateSettings(string connectionId, int lockProximity, int unlockProximity, int lockTimeout)
        {
            try
            {
                if (lockProximity < 0 || lockProximity > 100)
                    throw new ArgumentOutOfRangeException("Lock proximity must be in range from 0 to 100");

                if (unlockProximity < 0 || unlockProximity > 100)
                    throw new ArgumentOutOfRangeException("Unlock proximity must be in range from 0 to 100");

                if (unlockProximity <= lockProximity)
                    throw new ArgumentOutOfRangeException("Unlock proximity must be equal or lower than lock proximity");

                if (lockTimeout < 0)
                    throw new ArgumentOutOfRangeException("Lock delay seconds must be more than 0.");

                WriteLine($"{connectionId} monitor settings set to lock:{lockProximity}, unlock:{unlockProximity}, " +
                    $"lock_timeout:{lockTimeout}, proximity_timeout:{lockTimeout*2}");

                return true;
            }
            catch(Exception ex)
            {
                WriteLine($"Validation settings {connectionId} failed: {ex.Message}", LogErrorSeverity.Error);
                return false;
            }
        }

        public int GetLockProximity(string connectionId)
        {
            var deviceSettings = _userProximitySettings.GetProximitySettings(connectionId);
            return deviceSettings.LockProximity;
        }

        public int GetUnlockProximity(string connectionId)
        {
            var deviceSettings = _userProximitySettings.GetProximitySettings(connectionId);
            return deviceSettings.UnlockProximity;
        }

        public int GetLockTimeout(string connectionId)
        {
            return SdkConfig.DefaultLockTimeout;
        }

        public int GetProximityTimeout(string connectionId)
        {
            return GetLockProximity(connectionId)*2;
        }

        public bool IsEnabledLockByProximity(string connectionId)
        {
            var deviceSettings = _userProximitySettings.GetProximitySettings(connectionId);
            return deviceSettings.EnabledLockByProximity;
        }

        public bool IsEnabledUnlock(string connectionId)
        {
            var deviceSettings = _userProximitySettings.GetProximitySettings(connectionId);
            return deviceSettings.EnabledUnlockByProximity;
        }

        public bool IsDisabledUnlockByProximity(string connectionId)
        {
            var deviceSettings = _userProximitySettings.GetProximitySettings(connectionId);
            return deviceSettings.DisabledDisplayAuto;
        }
    }
}
