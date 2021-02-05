using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity.Interfaces;
using HideezMiddleware.ApplicationModeProvider;
using System.Collections.Generic;

namespace HideezMiddleware.Settings.SettingsProvider
{
    public class DeviceProximitySettingsProviderFactory: IProximitySettingsProviderFactory
    {
        private readonly ApplicationMode _applicationMode;
        private readonly ISettingsManager<ProximitySettings> _enterpriseProximitySettingsManager;
        private readonly ISettingsManager<UserProximitySettings> _userDevicesProximitySettingsManager;
        private readonly ILog _log;

        private readonly Dictionary<string, IDeviceProximitySettingsProvider> _providers = new Dictionary<string, IDeviceProximitySettingsProvider>();

        public DeviceProximitySettingsProviderFactory(
            ApplicationMode applicationMode,
            ISettingsManager<ProximitySettings> enterpriseProximitySettingsManager,
            ISettingsManager<UserProximitySettings> userDevicesProximitySettingsManager,
            ILog log)
        {
            _applicationMode = applicationMode;
            _enterpriseProximitySettingsManager = enterpriseProximitySettingsManager;
            _userDevicesProximitySettingsManager = userDevicesProximitySettingsManager;
            _log = log;
        }

        public IDeviceProximitySettingsProvider GetProximitySettingsProvider(string deviceId)
        {
            if (_providers.ContainsKey(deviceId))
                return _providers[deviceId];
            else
            {
                var provider = new DeviceProximitySettingsProvider(deviceId, _applicationMode, _enterpriseProximitySettingsManager, _userDevicesProximitySettingsManager, _log);
                _providers.Add(deviceId, provider);
                return provider;
            }
        }
    }
}
