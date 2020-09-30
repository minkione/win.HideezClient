using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Local;
using HideezMiddleware.ScreenActivation;
using HideezMiddleware.Settings;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class ConnectionFlowProcessorFactory
    {
        private readonly BleDeviceManager _deviceManager;
        private readonly BondManager _bondManager;
        private readonly IHesAppConnection _hesConnection;
        private readonly IWorkstationUnlocker _workstationUnlocker;
        private readonly IScreenActivator _screenActivator;
        private readonly IClientUiManager _ui;
        private readonly IHesAccessManager _hesAccessManager;
        private readonly ISettingsManager<ServiceSettings> _serviceSettingsManager;
        private readonly ILocalDeviceInfoCache _localDeviceInfoCache;
        private readonly ILog _log;

        public ConnectionFlowProcessorFactory(
            BleDeviceManager deviceManager,
            BondManager bondManager,
            IHesAppConnection hesConnection,
            IWorkstationUnlocker workstationUnlocker,
            IScreenActivator screenActivator,
            IClientUiManager ui,
            IHesAccessManager hesAccessManager,
            ISettingsManager<ServiceSettings> serviceSettingsManager, 
            ILocalDeviceInfoCache localDeviceInfoCache,
            ILog log)
        {
            _deviceManager = deviceManager;
            _bondManager = bondManager;
            _hesConnection = hesConnection;
            _workstationUnlocker = workstationUnlocker;
            _screenActivator = screenActivator;
            _ui = ui;
            _hesAccessManager = hesAccessManager;
            _serviceSettingsManager = serviceSettingsManager;
            _localDeviceInfoCache = localDeviceInfoCache;
            _log = log;
        }

        public ConnectionFlowProcessor Create()
        {
            var flowSubprocessors = new ConnectionFlowProcessor.ConnectionFlowSubprocessorsStruct()
            {
                PermissionsCheckProcessor = new PermissionsCheckProcessor(_hesAccessManager, _serviceSettingsManager),
                VaultConnectionProcessor = new VaultConnectionProcessor(_ui, _bondManager, _deviceManager, _log),
                LicensingProcessor = new LicensingProcessor(_hesConnection, _ui, _log),
                StateUpdateProcessor = new StateUpdateProcessor(_hesConnection, _ui, _log),
                ActivationProcessor = new ActivationProcessor(_hesConnection, _ui, _log),
                AccountsUpdateProcessor = new AccountsUpdateProcessor(_hesConnection, _log),
                MasterkeyProcessor = new VaultAuthorizationProcessor(_hesConnection, _ui, _log),
                UserAuthorizationProcessor = new UserAuthorizationProcessor(_ui, _log),
                UnlockProcessor = new UnlockProcessor(_ui, _workstationUnlocker, _log),
                CacheVaultInfoProcessor = new CacheVaultInfoProcessor(_localDeviceInfoCache, _log)
            };

            return new ConnectionFlowProcessor(
                _deviceManager, 
                _hesConnection, 
                _workstationUnlocker, 
                _screenActivator, 
                _ui, 
                _hesAccessManager, 
                _serviceSettingsManager, 
                flowSubprocessors, 
                _log);
        }
    }
}
