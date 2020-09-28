using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Device;
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
        readonly VaultConnectionProcessor _vaultConnectionProcessor;
        readonly LicensingProcessor _licensingProcessor;
        readonly StateUpdateProcessor _stateUpdateProcessor;
        readonly ActivationProcessor _activationProcessor;
        readonly AccountsUpdateProcessor _accountsUpdateProcessor;
        readonly VaultAuthorizationProcessor _masterkeyProcessor;
        readonly UserAuthorizationProcessor _userAuthorizationProcessor;
        readonly UnlockProcessor _unlockProcessor;
        // ...

        int _isConnecting = 0;
        CancellationTokenSource _cts;

        public event EventHandler<string> Started;
        public event EventHandler<IDevice> DeviceFinilizingMainFlow;
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
            // Cancel the workflow if workstation access was retracted on HES
            Cancel("Workstation access retracted");
        }

        void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            // Cancel the workflow if session switches to an unlocked (or different one)
            // Keep in mind, that workflow can cancel itself due to successful workstation unlock
            Cancel("Session switched");
        }

        void OnVaultDisconnectedDuringFlow(object sender, EventArgs e)
        {
            // Cancel the workflow if the vault disconnects
            Cancel("Vault unexpectedly disconnected");
        }

        void OnCancelledByVaultButton(object sender, EventArgs e)
        {
            // Cancel the workflow if the user pressed the cancel button (long button press)
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

            var flowId = Guid.NewGuid().ToString();
            Started?.Invoke(this, flowId);

            bool workflowFinishedSuccessfully = false;
            bool deleteVaultBond = false;
            string errorMessage = null;
            IDevice device = null;

            try
            {
                await _ui.SendNotification(string.Empty, mac);

                _permissionsCheckProcessor.CheckPermissions();

                // Start periodic screen activator to raise the "curtain"
                if (WorkstationHelper.IsActiveSessionLocked())
                {
                    _screenActivator?.ActivateScreen();
                    _screenActivator?.StartPeriodicScreenActivation(0);

                    await new WaitWorkstationUnlockerConnectProc(_workstationUnlocker)
                        .Run(SdkConfig.WorkstationUnlockerConnectTimeout, ct);
                }

                device = await _vaultConnectionProcessor.ConnectVault(mac, rebondOnConnectionFail, ct);
                device.Disconnected += OnVaultDisconnectedDuringFlow;
                device.OperationCancelled += OnCancelledByVaultButton;

                await _vaultConnectionProcessor.WaitVaultInitialization(mac, device, ct);

                if (device.IsBoot)
                    throw new WorkflowException(TranslationSource.Instance["ConnectionFlow.Error.VaultInBootloaderMode"]);

                device.SetUserProperty(CustomProperties.HW_CONNECTION_STATE_PROP, HwVaultConnectionState.Initializing);

                var vaultInfo = await _hesConnection.UpdateDeviceProperties(new HwVaultInfoFromClientDto(device), true);

                // Todo: 
                // Save owner and email from vault info into local cache. 
                // If vault info is not available, load owner and email from local cache

                await _licensingProcessor.CheckLicense(device, vaultInfo, ct);
                vaultInfo = await _stateUpdateProcessor.UpdateDeviceState(device, vaultInfo, ct);
                vaultInfo = await _activationProcessor.ActivateVault(device, vaultInfo, ct);

                await _masterkeyProcessor.AuthVault(device, ct);

                var osAccUpdateTask = _accountsUpdateProcessor.UpdateAccounts(device, vaultInfo, true);
                if (_workstationUnlocker.IsConnected && WorkstationHelper.IsActiveSessionLocked() && tryUnlock)
                {
                    await Task.WhenAll(_userAuthorizationProcessor.AuthorizeUser(device, ct), osAccUpdateTask);

                    _screenActivator?.StopPeriodicScreenActivation();
                    await _unlockProcessor.UnlockWorkstation(device, flowId, onUnlockAttempt, ct);
                }
                else
                    await osAccUpdateTask;

                device.SetUserProperty(CustomProperties.HW_CONNECTION_STATE_PROP, HwVaultConnectionState.Finalizing);
                WriteLine($"Finalizing main workflow: ({device.Id})");
                DeviceFinilizingMainFlow?.Invoke(this, device);
                
                await _accountsUpdateProcessor.UpdateAccounts(device, vaultInfo, false);

                device.SetUserProperty(CustomProperties.HW_CONNECTION_STATE_PROP, HwVaultConnectionState.Online);

                await _hesConnection.UpdateDeviceProperties(new HwVaultInfoFromClientDto(device), false);

                workflowFinishedSuccessfully = true;
            }
            catch (HideezException ex)
            {
                switch (ex.ErrorCode)
                {
                    case HideezErrorCode.DeviceIsLocked:
                    case HideezErrorCode.DeviceNotAssignedToUser:
                    case HideezErrorCode.HesDeviceNotFound:
                    case HideezErrorCode.HesDeviceCompromised:
                    case HideezErrorCode.DeviceHasBeenWiped:
                        // There errors require bond removal
                        deleteVaultBond = true;
                        errorMessage = HideezExceptionLocalization.GetErrorAsString(ex);
                        break;
                    case HideezErrorCode.ButtonConfirmationTimeout:
                    case HideezErrorCode.GetPinTimeout:
                    case HideezErrorCode.GetActivationCodeTimeout:
                        // Silent handling
                        WriteLine(ex);
                        break;
                    default:
                        errorMessage = HideezExceptionLocalization.GetErrorAsString(ex);
                        break;
                }
            }
            catch (VaultFailedToAuthorizeException ex)
            {
                // User should never receive this error unless there is a bug in algorithm 
                errorMessage = HideezExceptionLocalization.GetErrorAsString(ex);
            }
            catch (WorkstationUnlockFailedException ex)
            {
                // Silent handling of failed workstation unlock
                // The actual message will be displayed by credential provider
                WriteLine(ex);
            }
            catch (OperationCanceledException ex)
            {
                // Silent cancelation handling
                WriteLine(ex);
            }
            catch (TimeoutException ex)
            {
                // Silent timeout handling
                WriteLine(ex);
            }
            catch (Exception ex)
            {
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

            await WorkflowCleanup(errorMessage, mac, device, workflowFinishedSuccessfully, deleteVaultBond);

            Finished?.Invoke(this, flowId);

            WriteLine($"Main workflow end {mac}");
        }

        async Task WorkflowCleanup(string errorMessage, string mac, IDevice device, bool workflowFinishedSuccessfully, bool deleteVaultBond)
        {
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
                    if (workflowFinishedSuccessfully)
                    {
                        WriteLine($"Successfully finished the main workflow: ({device.Id})");
                        DeviceFinishedMainFlow?.Invoke(this, device);
                    }
                    else if (deleteVaultBond)
                    {
                        WriteLine($"Mainworkflow critical error, Removing ({device.Id})");
                        await _deviceManager.Remove(device);
                    }
                    else
                    {
                        WriteLine($"Main workflow failed, Disconnecting ({device.Id})");
                        await _deviceManager.DisconnectDevice(device);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex, LogErrorSeverity.Error);
            }
        }
    }
}
