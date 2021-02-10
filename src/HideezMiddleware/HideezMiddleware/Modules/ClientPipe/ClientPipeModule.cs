using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.ClientManagement;
using HideezMiddleware.DeviceConnection.Workflow;
using HideezMiddleware.IPC.DTO;
using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.IPC.Messages;
using HideezMiddleware.Modules.DeviceManagement.Messages;
using HideezMiddleware.Settings;
using Meta.Lib.Modules.PubSub;
using Meta.Lib.Modules.PubSub.Messages;
using Microsoft.Win32;
using System;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;

namespace HideezMiddleware.Modules.ClientPipe
{
    public sealed class ClientPipeModule : ModuleBase
    {
        readonly DeviceManager _deviceManager;
        readonly ServiceClientUiManager _clientUi;
        readonly ISettingsManager<ServiceSettings> _serviceSettingsManager;
        readonly SessionUnlockMethodMonitor _sessionUnlockMethodMonitor;
        readonly StatusManager _statusManager;
        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly PipeDeviceConnectionManager _pipeDeviceConnectionManager;

        public ClientPipeModule(DeviceManager deviceManager, 
            ServiceClientUiManager clientUiProxy, 
            ISettingsManager<ServiceSettings> serviceSettingsManager,
            SessionUnlockMethodMonitor sessionUnlockMethodMonitor,
            StatusManager statusManager,
            ConnectionFlowProcessor connectionFlowProcessor,
            IMetaPubSub messenger, 
            ILog log)
            : base(messenger, nameof(ClientPipeModule), log)
        {
            _deviceManager = deviceManager;
            _clientUi = clientUiProxy;
            _serviceSettingsManager = serviceSettingsManager;
            _sessionUnlockMethodMonitor = sessionUnlockMethodMonitor;
            _statusManager = statusManager;
            _connectionFlowProcessor = connectionFlowProcessor;
            _pipeDeviceConnectionManager = new PipeDeviceConnectionManager(deviceManager, messenger, log);

            var pipeName = "HideezServicePipe";
            _messenger.StartServer(pipeName, () =>
            {
                try
                {
                    WriteLine("Custom pipe config started");
                    var pipeSecurity = new PipeSecurity();
                    pipeSecurity.AddAccessRule(new PipeAccessRule(
                        new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                        PipeAccessRights.FullControl,
                        AccessControlType.Allow));

                    var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 32,
                        PipeTransmissionMode.Message, PipeOptions.Asynchronous, 4096, 4096, pipeSecurity);

                    WriteLine("Custom pipe config successful");
                    return pipe;
                }
                catch (Exception ex)
                {
                    WriteLine("Custom pipe config failed.", ex);
                    return null;
                }
            });

            _serviceSettingsManager.SettingsChanged += ServiceSettingsManager_SettingsChanged;

            _connectionFlowProcessor.DeviceFinishedMainFlow += ConnectionFlowProcessor_DeviceFinishedMainFlow;

            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;

            _messenger.Subscribe<DeviceManager_DeviceAddedMessage>(OnDeviceAdded);
            _messenger.Subscribe<DeviceManager_DeviceRemovedMessage>(OnDeviceRemoved);

            _messenger.Subscribe<RemoteClientConnectedEvent>(OnClientConnected);
            _messenger.Subscribe<RemoteClientDisconnectedEvent>(OnClientDisconnected);

            _messenger.Subscribe<LoginClientRequestMessage>(OnClientLogin);
            _messenger.Subscribe<RefreshServiceInfoMessage>(OnRefreshServiceInfo);
        }

        private void Error(Exception ex, string message = "")
        {
            WriteLine(message, ex);
        }

        private void Error(string message)
        {
            WriteLine(message, LogErrorSeverity.Error);
        }

        private DeviceDTO[] GetDevices()
        {
            try
            {
                return _deviceManager.Devices.Select(d => new DeviceDTO(d)).ToArray();
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        async Task RefreshServiceInfo() // Todo: error handling
        {
            await _statusManager.SendStatusToUI();
            
            await SafePublish(new DevicesCollectionChangedMessage(GetDevices()));

            await SafePublish(new ServiceSettingsChangedMessage(_serviceSettingsManager.Settings.EnableSoftwareVaultUnlock, RegistrySettings.GetHesAddress(this)));

            await SafePublish(new RefreshStatusMessage());
        }

        async void ServiceSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<ServiceSettings> e)
        {
            await RefreshServiceInfo();
        }

        async void ConnectionFlowProcessor_DeviceFinishedMainFlow(object sender, IDevice e)
        {
            await SafePublish(new DeviceFinishedMainFlowMessage(new DeviceDTO(e)));
        }

        async void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            if (reason == SessionSwitchReason.SessionUnlock || reason == SessionSwitchReason.SessionLogon)
                await SafePublish(new WorkstationUnlockedMessage(_sessionUnlockMethodMonitor.GetUnlockMethod() == SessionSwitchSubject.NonHideez));
        }

        private async Task OnDeviceAdded(DeviceManager_DeviceAddedMessage msg)
        {
            await SafePublish(new DevicesCollectionChangedMessage(GetDevices()));
        }

        private async Task OnDeviceRemoved(DeviceManager_DeviceRemovedMessage msg)
        {
            await SafePublish(new DevicesCollectionChangedMessage(GetDevices()));
        }

        Task OnClientConnected(RemoteClientConnectedEvent arg)
        {
            WriteLine($">>>>>> AttachClient, {arg.TotalClientsCount} remaining clients", LogErrorSeverity.Debug);
            return Task.CompletedTask;
        }

        Task OnClientDisconnected(RemoteClientDisconnectedEvent arg)
        {
            WriteLine($">>>>>> DetachClient, {arg.TotalClientsCount} remaining clients", LogErrorSeverity.Debug);
            return Task.CompletedTask;
        }

        // This message is sent by client when its ready to receive messages from server
        // Upon receiving LoginClientRequestMessage we sent to the client all currently available information
        // Previously the client had to ask for each bit separatelly. Now we instead send it upon connection establishment
        // The updates in sent information are sent separatelly
        async Task OnClientLogin(LoginClientRequestMessage arg)
        {
            WriteLine($"Service client login");

            await RefreshServiceInfo();

            // Client may have only been launched after we already sent an event about workstation unlock
            // This may happen if we are login into the new session where application is not running during unlock but loads afterwards
            // To avoid the confusion, we resend the event about latest unlock method to every client that connects to service
            if (_deviceManager.Devices.Count() == 0)
                await SafePublish(new WorkstationUnlockedMessage(_sessionUnlockMethodMonitor.GetUnlockMethod() == SessionSwitchSubject.NonHideez));

        }

        async Task OnRefreshServiceInfo(RefreshServiceInfoMessage arg)
        {
            await RefreshServiceInfo();
        }

    }
}
