using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceLogging;
using HideezMiddleware.ScreenActivation;
using HideezMiddleware.Utils.WorkstationHelper;

namespace HideezMiddleware.DeviceConnection.Workflow.ConnectionFlow
{
    public class StandaloneConnectionFlowProcessorFactory
    {
        private readonly DeviceManager _deviceManager;
        private readonly BondManager _bondManager;
        private readonly IWorkstationUnlocker _workstationUnlocker;
        private readonly IScreenActivator _screenActivator;
        private readonly IClientUiManager _ui;
        private readonly IWorkstationHelper _workstationHelper;
        private readonly DeviceLogManager _deviceLogManager;
        private readonly ILog _log;

        public StandaloneConnectionFlowProcessorFactory(
            DeviceManager deviceManager,
            BondManager bondManager,
            IWorkstationUnlocker workstationUnlocker,
            IScreenActivator screenActivator,
            IClientUiManager ui,
            IWorkstationHelper workstationHelper,
            DeviceLogManager deviceLogManager,
            ILog log)
        {
            _deviceManager = deviceManager;
            _bondManager = bondManager;
            _workstationUnlocker = workstationUnlocker;
            _screenActivator = screenActivator;
            _ui = ui;
            _workstationHelper = workstationHelper;
            _deviceLogManager = deviceLogManager;
            _log = log;
        }

        public StandaloneConnectionFlowProcessor Create()
        {
            var flowSubprocessors = new StandaloneConnectionFlowProcessor.StandaloneConnectionFlowSubprocessorsStruct()
            {
                VaultConnectionProcessor = new VaultConnectionProcessor(_ui, _bondManager, _deviceManager, _log),
                MasterkeyProcessor = new StandaloneVaultAuthorizationProcessor(_ui, _log),
                UserAuthorizationProcessor = new UserAuthorizationProcessor(_ui, _log),
                UnlockProcessor = new UnlockProcessor(_ui, _workstationUnlocker, _log),
            };

            return new StandaloneConnectionFlowProcessor(
                _deviceManager,
                _workstationUnlocker,
                _screenActivator,
                _ui,
                flowSubprocessors,
                _workstationHelper,
                _deviceLogManager,
                _log);
        }
    }
}
