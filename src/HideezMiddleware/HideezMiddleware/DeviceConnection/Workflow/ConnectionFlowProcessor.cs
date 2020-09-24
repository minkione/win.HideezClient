using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Local;
using HideezMiddleware.Localize;
using HideezMiddleware.ScreenActivation;
using HideezMiddleware.Settings;
using HideezMiddleware.Tasks;
using Microsoft.Win32;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class ConnectionFlowProcessor : Logger
    {
        // Todo: Move constant property names to another class
        public const string FLOW_FINISHED_PROP = "MainFlowFinished";
        public const string OWNER_NAME_PROP = "OwnerName";
        public const string OWNER_EMAIL_PROP = "OwnerEmail";

        readonly IBleConnectionManager _connectionManager;
        readonly BleDeviceManager _deviceManager;
        readonly IWorkstationUnlocker _workstationUnlocker;
        readonly IScreenActivator _screenActivator;
        readonly UiProxyManager _ui;
        readonly IHesAppConnection _hesConnection;
        readonly ILocalDeviceInfoCache _localDeviceInfoCache;
        readonly IHesAccessManager _hesAccessManager;
        readonly BondManager _bondManager;
        readonly ISettingsManager<ServiceSettings> _serviceSettingsManager;

        // Todo: initialize
        readonly PermissionsCheckProcessor _permissionsCheckProcessor;
        readonly LicensingProcessor _licensingProcessor;
        readonly StateUpdateProcessor _stateUpdateProcessor;
        readonly ActivationProcessor _activationProcessor;
        readonly AccountsUpdateProcessor _accountsUpdateProcessor;
        readonly VaultAuthorizationProcessor _masterkeyProcessor;
        readonly UserAuthorizationProcessor _userAuthorizationProcessor;
        readonly UnlockProcessor _unlockProcessor;

        int _isConnecting = 0;
        CancellationTokenSource _cts;

        string _flowId = string.Empty;

        public event EventHandler<string> Started;
        public event EventHandler<IDevice> DeviceFinishedMainFlow;
        public event EventHandler<string> Finished;

        public ConnectionFlowProcessor(
            IBleConnectionManager connectionManager,
            BleDeviceManager deviceManager,
            IHesAppConnection hesConnection,
            BondManager bondManager,
            IWorkstationUnlocker workstationUnlocker,
            IScreenActivator screenActivator,
            UiProxyManager ui,
            ILocalDeviceInfoCache localDeviceInfoCache,
            IHesAccessManager hesAccessManager,
            ISettingsManager<ServiceSettings> serviceSettingsManager,
            ILog log)
            : base(nameof(ConnectionFlowProcessor), log)
        {
            _connectionManager = connectionManager;
            _deviceManager = deviceManager;
            _workstationUnlocker = workstationUnlocker;
            _screenActivator = screenActivator;
            _ui = ui;
            _hesConnection = hesConnection;
            _bondManager = bondManager;
            _localDeviceInfoCache = localDeviceInfoCache;
            _hesAccessManager = hesAccessManager;
            _serviceSettingsManager = serviceSettingsManager;

            _hesAccessManager.AccessRetractedEvent += HesAccessManager_AccessRetractedEvent;
            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;
        }

        void HesAccessManager_AccessRetractedEvent(object sender, EventArgs e)
        {
            // cancel the workflow if workstation access was retracted on HES
            Cancel("Workstation access retracted");
        }

        void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            // cancel the workflow if session switches to an unlocked (or different one)
            // TODO: MainFlow can cancel itself after successful unlock
            Cancel("Session switched");
        }

        void OnVaultDisconnectedDuringFlow(object sender, EventArgs e)
        {
            // cancel the workflow if the vault disconnects
            Cancel("Vault unexpectedly disconnected");
        }

        void OnCancelledByVaultButton(object sender, EventArgs e)
        {
            // cancel the workflow if the user pressed the cancel button (long button press)
            Cancel("User pressed the cancel button");
        }

        public void Cancel(string reason)
        {
            WriteLine($"Canceling; {reason}");
            _cts?.Cancel();
        }

        public async Task Connect(string mac)
        {
            // ignore, if already performing workflow for any device
            if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0)
            {
                try
                {
                    _cts = new CancellationTokenSource();
                    await MainWorkflow(mac, false, false, null, _cts.Token);
                }
                finally
                {
                    _cts.Dispose();
                    _cts = null;

                    Interlocked.Exchange(ref _isConnecting, 0);
                }
            }

        }

        public async Task ConnectAndUnlock(string mac, Action<WorkstationUnlockResult> onSuccessfulUnlock)
        {
            // ignore, if already performing workflow for any device
            if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0)
            {
                try
                {
                    _cts = new CancellationTokenSource();
                    await MainWorkflow(mac, true, true, onSuccessfulUnlock, _cts.Token);
                }
                finally
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;

                    Interlocked.Exchange(ref _isConnecting, 0);
                }
            }
        }

        async Task MainWorkflow(string mac, bool rebondOnConnectionFail, bool tryUnlock, Action<WorkstationUnlockResult> onUnlockAttempt, CancellationToken ct)
        {
            // Ignore MainFlow requests for devices that are already connected
            // IsConnected-true indicates that device already finished main flow or is in progress
            var existingDevice = _deviceManager.Find(mac, (int)DefaultDeviceChannel.Main);
            if (existingDevice != null && existingDevice.IsConnected && !WorkstationHelper.IsActiveSessionLocked())
                return;

            WriteLine($"Started main workflow ({mac})");

            _flowId = Guid.NewGuid().ToString();
            Started?.Invoke(this, _flowId);

            bool success = false;
            bool criticalError = false;
            string errorMessage = null;
            IDevice device = null;

            try
            {
                await _ui.SendNotification("", mac);

                _permissionsCheckProcessor.CheckPermissions();

                // Start periodic screen activator to raise the "curtain"
                if (WorkstationHelper.IsActiveSessionLocked())
                {
                    _screenActivator?.ActivateScreen();
                    _screenActivator?.StartPeriodicScreenActivation(0);

                    await new WaitWorkstationUnlockerConnectProc(_workstationUnlocker)
                        .Run(SdkConfig.WorkstationUnlockerConnectTimeout, ct);
                }


                device = await ConnectDevice(mac, rebondOnConnectionFail, ct);

                device.Disconnected += OnVaultDisconnectedDuringFlow;
                device.OperationCancelled += OnCancelledByVaultButton;

                await WaitDeviceInitialization(mac, device, ct);

                if (device.IsBoot)
                    throw new HideezException(HideezErrorCode.DeviceInBootloaderMode);

                // ....
                // todo: set vault state to initializing
                device.SetUserProperty("MainflowConnectionState", Hideez.SDK.Communication.HES.DTO.ConnectionState.Initializing);
                // ....

                var vaultInfo = await _hesConnection.UpdateDeviceProperties(new HwVaultInfoFromClientDto(device), true);

                await _licensingProcessor.CheckLicense(device, vaultInfo, ct);
                vaultInfo = await _stateUpdateProcessor.UpdateDeviceState(device, vaultInfo, ct);
                await device.RefreshDeviceInfo();

                await _activationProcessor.ActivateVault(device, ct);
                await device.RefreshDeviceInfo();

                await _masterkeyProcessor.ActivateVault(device, ct);
                await device.RefreshDeviceInfo(); 

                await Task.WhenAll(_accountsUpdateProcessor.UpdateAccounts(device, vaultInfo, true), _userAuthorizationProcessor.AuthorizeUser(device));

                await _unlockProcessor.UnlockWorkstation(device); // todo
                
                await _accountsUpdateProcessor.UpdateAccounts(device, vaultInfo, false); // todo

                // ....
                // todo: user prop name
                device.SetUserProperty("MainflowConnectionState", Hideez.SDK.Communication.HES.DTO.ConnectionState.Online);
                // ....

                await _hesConnection.UpdateDeviceProperties(new HwVaultInfoFromClientDto(device), false);
            }
            catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.HesDeviceNotFound)
            {
                criticalError = true;
                errorMessage = HideezExceptionLocalization.GetErrorAsString(ex);
            }
            finally
            {
                if (device != null)
                {
                    device.Disconnected -= OnVaultDisconnectedDuringFlow;
                    device.OperationCancelled -= OnCancelledByVaultButton;
                }
                _screenActivator?.StopPeriodicScreenActivation();
            }


            // Cleanup
            try
            {
                await _ui.HidePinUi();

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    WriteLine(errorMessage);
                    await _ui.SendError(errorMessage, mac);
                }

                if (device != null)
                {
                    if (criticalError)
                    {
                        WriteLine($"Mainworkflow critical error, Removing ({device.Id})");
                        await _deviceManager.Remove(device);
                    }
                    else if (!success)
                    {
                        WriteLine($"Main workflow failed, Disconnecting ({device.Id})");
                        await _deviceManager.DisconnectDevice(device);
                    }
                    else
                    {
                        WriteLine($"Successfully finished the main workflow: ({device.Id})");
                        device.SetUserProperty(FLOW_FINISHED_PROP, true);
                        DeviceFinishedMainFlow?.Invoke(this, device);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex, LogErrorSeverity.Error);
            }

            Finished?.Invoke(this, _flowId);
            _flowId = string.Empty;

            WriteLine($"Main workflow end {mac}");
        }


        async Task<IDevice> ConnectDevice(string mac, bool rebondOnFail, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (_bondManager.Exists(mac))
                await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage1"], mac);
            else await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage1.PressButton"], mac);

            bool ltkErrorOccured = false;
            IDevice device = null;
            try
            {
                device = await _deviceManager.ConnectDevice(mac, SdkConfig.ConnectDeviceTimeout);
            }
            catch (Exception ex) // Thrown when LTK error occurs in csr
            {
                WriteLine(ex);
                ltkErrorOccured = true;
            }

            if (device == null)
            {
                ct.ThrowIfCancellationRequested();

                string ltk = "";
                if (ltkErrorOccured)
                {
                    ltk = "LTK error.";
                    ltkErrorOccured = false;
                }
                if (_bondManager.Exists(mac))
                    await _ui.SendNotification(ltk + TranslationSource.Instance["ConnectionFlow.Connection.Stage2"], mac);
                else await _ui.SendNotification(ltk + TranslationSource.Instance["ConnectionFlow.Connection.Stage2.PressButton"], mac);

                try
                {
                    device = await _deviceManager.ConnectDevice(mac, SdkConfig.ConnectDeviceTimeout / 2);
                }
                catch (Exception ex) // Thrown when LTK error occurs in csr
                {
                    WriteLine(ex);
                    ltkErrorOccured = true;
                }

                if (device == null && rebondOnFail)
                {
                    ct.ThrowIfCancellationRequested();

                    // remove the bond and try one more time
                    await _deviceManager.RemoveByMac(mac);

                    if (ltkErrorOccured)
                        await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage3.LtkError.PressButton"], mac); // TODO: Fix LTK error in CSR
                    else
                        await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Connection.Stage3.PressButton"], mac);

                    device = await _deviceManager.ConnectDevice(mac, SdkConfig.ConnectDeviceTimeout);
                }
            }

            if (device == null)
                throw new Exception(TranslationSource.Instance.Format("ConnectionFlow.Connection.ConnectionFailed", mac));

            return device;
        }

        async Task WaitDeviceInitialization(string mac, IDevice device, CancellationToken ct)
        {
            await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Initialization.WaitingInitializationMessage"], mac);

            if (!await device.WaitInitialization(SdkConfig.DeviceInitializationTimeout, ct))
                throw new Exception(TranslationSource.Instance.Format("ConnectionFlow.Initialization.InitializationFailed", mac));

            if (device.IsErrorState)
            {
                await _deviceManager.Remove(device);
                throw new Exception(TranslationSource.Instance.Format("ConnectionFlow.Initialization.DeviceInitializationError", mac, device.ErrorMessage));
            }
        }

    }
}
