using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Tasks;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.Local;
using HideezMiddleware.ScreenActivation;
using HideezMiddleware.Tasks;
using Microsoft.Win32;

namespace HideezMiddleware
{
    public class ConnectionFlowProcessor : Logger
    {
        public const string FLOW_FINISHED_PROP = "MainFlowFinished";
        public const string OWNER_NAME_PROP = "OwnerName";
        public const string OWNER_EMAIL_PROP = "OwnerEmail";

        readonly IBleConnectionManager _connectionManager;
        readonly BleDeviceManager _deviceManager;
        readonly IWorkstationUnlocker _workstationUnlocker;
        readonly IScreenActivator _screenActivator;
        readonly UiProxyManager _ui;
        readonly HesAppConnection _hesConnection;
        readonly ILocalDeviceInfoCache _localDeviceInfoCache;

        int _isConnecting = 0;
        string _infNid = string.Empty; // Notification Id, which must be the same for the entire duration of MainWorkflow
        string _errNid = string.Empty; // Error Notification Id
        CancellationTokenSource _cts;

        string _flowId = string.Empty;

        public event EventHandler<string> Started;
        public event EventHandler<IDevice> DeviceFinishedMainFlow;
        public event EventHandler<string> Finished;

        public ConnectionFlowProcessor(
            IBleConnectionManager connectionManager,
            BleDeviceManager deviceManager,
            HesAppConnection hesConnection,
            IWorkstationUnlocker workstationUnlocker,
            IScreenActivator screenActivator,
            UiProxyManager ui,
            ILocalDeviceInfoCache localDeviceInfoCache,
            ILog log)
            : base(nameof(ConnectionFlowProcessor), log)
        {
            _connectionManager = connectionManager;
            _deviceManager = deviceManager;
            _workstationUnlocker = workstationUnlocker;
            _screenActivator = screenActivator;
            _ui = ui;
            _hesConnection = hesConnection;
            _localDeviceInfoCache = localDeviceInfoCache;

            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;
        }

        void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            // TODO: MainFlow can cancel itself after successful unlock
            Cancel();
        }

        void OnDeviceDisconnectedDuringFlow(object sender, EventArgs e)
        {
            WriteLine("Canceling because disconnect");
            // cancel the workflow if the device disconnects
            Cancel();
        }

        void OnUserCancelledByButton(object sender, EventArgs e)
        {
            WriteLine("Canceling because the user pressed the cancel button");
            // cancel the workflow if the user have pressed the cancel (long button press)
            Cancel();
        }

        public void Cancel()
        {
            _cts?.Cancel();
        }

        public async Task ConnectAndUnlock(string mac, Action<WorkstationUnlockResult> onSuccessfulUnlock)
        {
            // ignore, if workflow for any device already initialized
            if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0)
            {
                try
                {
                    _cts = new CancellationTokenSource();
                    await MainWorkflow(mac, true, true, onSuccessfulUnlock, _cts.Token);
                }
                finally
                {
                    _cts.Dispose();
                    _cts = null;

                    Interlocked.Exchange(ref _isConnecting, 0);
                }
            }
        }

        public async Task Connect(string mac)
        {
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

        async Task MainWorkflow(string mac, bool rebondOnConnectionFail, bool tryUnlock, Action<WorkstationUnlockResult> onUnlockAttempt, CancellationToken ct)
        {
            // Ignore MainFlow requests for devices that are already connected
            // IsConnected-true indicates that device already finished main flow or is in progress
            var existingDevice = _deviceManager.Find(mac, (int)DefaultDeviceChannel.Main);
            if (existingDevice != null && existingDevice.IsConnected && !WorkstationHelper.IsActiveSessionLocked())
                return;

            Debug.WriteLine(">>>>>>>>>>>>>>> MainWorkflow +++++++++++++++++++++++++");
            WriteLine($"Started main workflow ({mac})");

            _flowId = Guid.NewGuid().ToString();
            Started?.Invoke(this, _flowId);

            bool success = false;
            bool fatalError = false;
            string errorMessage = null;
            IDevice device = null;
            // Appending 'i' and 'e' allows us to differentiate between different notification kinds
            // while still allowing to replace outdated notifications for target device with ones created by subsequent calls
            // TODO: check if we need different ID's since we dont show more than one notification at a time anyway
            _infNid = mac + "_i";
            _errNid = mac + "_e";

            try
            {
                await _ui.SendNotification("", _infNid);
                await _ui.SendError("", _errNid);

                // start fetching the device info in the background
                var deviceInfoProc = new GetDeviceInfoFromHesProc(_hesConnection, mac, ct);
                var deviceInfoProcTask = deviceInfoProc.Run();

                if (WorkstationHelper.IsActiveSessionLocked())
                {
                    _screenActivator?.ActivateScreen();
                    _screenActivator?.StartPeriodicScreenActivation(0);

                    await new WaitWorkstationUnlockerConnectProc(_workstationUnlocker)
                        .Run(SdkConfig.WorkstationUnlockerConnectTimeout, ct);
                }

                device = await ConnectDevice(mac, rebondOnConnectionFail, ct);

                device.Disconnected += OnDeviceDisconnectedDuringFlow;
                device.OperationCancelled += OnUserCancelledByButton;

                await WaitDeviceInitialization(mac, device, ct);

                if (device.IsBoot)
                    throw new HideezException(HideezErrorCode.DeviceInBootloaderMode);

                DeviceInfoDto deviceInfo = null;
                try
                {
                    deviceInfo = await deviceInfoProcTask;
                }
                catch (Exception ex)
                {
                    WriteLine("Non-fatal error occured while loading device info from HES", ex);
                }

                WriteLine($"Check if device is locked: {device.AccessLevel.IsLocked}");
                if (device.AccessLevel.IsLocked)
                {
                    // request HES to update this device
                    await _hesConnection.FixDevice(device, ct);
                    await device.RefreshDeviceInfo();
                }

                if (device.AccessLevel.IsLocked)
                    throw new HideezException(HideezErrorCode.DeviceIsLocked);

                if (deviceInfoProc.IsSuccessful)
                {
                    CacheAndUpdateDeviceOwner(device, deviceInfo);

                    WriteLine($"Device info retrieved. HasNewLicense: {deviceInfo.HasNewLicense}");
                    if (deviceInfo.HasNewLicense)
                    {
                        // License upload has the highest priority in connection flow. Without license other actions are impossible
                        await _ui.SendNotification("Updating device licenses...", _infNid);
                        await LicenseWorkflow(device, ct);
                    }
                }
                else
                {
                    WriteLine("Couldn't retrieve device info from HES. Using local device info.");
                    LoadLocalDeviceOwner(device);
                }

                WriteLine("Query licenses");
                var activeLicense = await device.QueryActiveLicense(SdkConfig.DefaultCommandTimeout);
                if (activeLicense.IsEmpty)
                    throw new HideezException(HideezErrorCode.ERR_NO_LICENSE);

                WriteLine($"Check if link is required: {device.AccessLevel.IsLinkRequired}");
                if (device.AccessLevel.IsLinkRequired)
                {
                    // request HES to update this device
                    await _hesConnection.FixDevice(device, ct);
                    await device.RefreshDeviceInfo();
                }

                if (device.AccessLevel.IsLocked)
                    throw new HideezException(HideezErrorCode.DeviceIsLocked);

                if (device.AccessLevel.IsLinkRequired)
                    throw new HideezException(HideezErrorCode.DeviceNotAssignedToUser);

                if (deviceInfo.NeedUpdate)
                {
                    // request HES to update this device
                    await _ui.SendNotification("Uploading new credentials to the device...", _infNid);
                    await _hesConnection.FixDevice(device, ct);
                    await device.RefreshDeviceInfo();
                }

                int timeout = SdkConfig.MainWorkflowTimeout;

                await MasterKeyWorkflow(device, ct);

                if (_workstationUnlocker.IsConnected && WorkstationHelper.IsActiveSessionLocked() && tryUnlock)
                {
                    if (await ButtonWorkflow(device, timeout, ct) && await PinWorkflow(device, timeout, ct))
                    {
                        if (!device.AccessLevel.IsLocked &&
                            !device.AccessLevel.IsButtonRequired &&
                            !device.AccessLevel.IsPinRequired)
                        {
                            await _ui.SendNotification("", _infNid);
                            await _ui.SendError("", _errNid);

                            var unlockResult = await TryUnlockWorkstation(device);
                            success = unlockResult.IsSuccessful;
                            onUnlockAttempt?.Invoke(unlockResult);
                        }
                    }
                }
                else if (WorkstationHelper.IsActiveSessionLocked())
                {
                    // Session is locked but workstation unlocker is not connected
                    success = false;
                }
                else
                {
                    success = true;
                }
            }
            catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.DeviceNotAssignedToUser)
            {
                fatalError = true;
                errorMessage = HideezExceptionLocalization.GetErrorAsString(ex);
            }
            catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.HesDeviceNotFound)
            {
                fatalError = true;
                errorMessage = HideezExceptionLocalization.GetErrorAsString(ex);
            }
            catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.DeviceIsLocked)
            {
                fatalError = true;
                errorMessage = HideezExceptionLocalization.GetErrorAsString(ex);
            }
            catch (HideezException ex)
            {
                if (ex.ErrorCode == HideezErrorCode.ButtonConfirmationTimeout ||
                    ex.ErrorCode == HideezErrorCode.GetPinTimeout)
                {
                    // Silent cancelation handling
                    WriteLine(ex);
                }
                else
                    errorMessage = HideezExceptionLocalization.GetErrorAsString(ex);
            }
            catch (OperationCanceledException ex)
            {
                // Silent cancelation handling
                WriteLine(ex);
            }
            catch (TimeoutException ex)
            {
                // Silent cancelation handling
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
                    device.Disconnected -= OnDeviceDisconnectedDuringFlow;
                    device.OperationCancelled -= OnUserCancelledByButton;
                }
                _screenActivator?.StopPeriodicScreenActivation();
            }

            // Cleanup
            try
            {
                await _ui.HidePinUi();
                await _ui.SendNotification("", _infNid);

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    WriteLine(errorMessage);
                    await _ui.SendError(errorMessage, _errNid);
                }

                if (device != null)
                {
                    if (fatalError)
                    {
                        WriteLine($"Fatal error: Remove ({device.Id})");
                        await _deviceManager.Remove(device);
                    }
                    else if (!success)
                    {
                        WriteLine($"Main workflow failed: Disconnect ({device.Id})");
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
                WriteLine(ex, LogErrorSeverity.Fatal);
            }
            finally
            {
                _infNid = string.Empty;
                _errNid = string.Empty;
            }

            Finished?.Invoke(this, _flowId);
            _flowId = string.Empty;

            Debug.WriteLine(">>>>>>>>>>>>>>> MainWorkflow ------------------------------");
            WriteLine($"Main workflow end {mac}");
        }

        async Task<WorkstationUnlockResult> TryUnlockWorkstation(IDevice device)
        {
            var result = new WorkstationUnlockResult();

            await _ui.SendNotification("Reading credentials from the device...", _infNid);
            await _ui.SendError("", _errNid);
            var credentials = await GetCredentials(device);

            // send credentials to the Credential Provider to unlock the PC
            await _ui.SendNotification("Unlocking the PC...", _infNid);
            await _ui.SendError("", _errNid); 
            result.IsSuccessful = await _workstationUnlocker
                .SendLogonRequest(credentials.Login, credentials.Password, credentials.PreviousPassword);

            result.AccountName = credentials.Name;
            result.AccountLogin = credentials.Login;
            result.DeviceMac = device.Mac;
            result.FlowId = _flowId;

            return result;
        }

        async Task MasterKeyWorkflow(IDevice device, CancellationToken ct)
        {
            if (!device.AccessLevel.IsMasterKeyRequired)
                return;

            ct.ThrowIfCancellationRequested();

            await _ui.SendNotification("Waiting for HES authorization...", _infNid);

            await _hesConnection.FixDevice(device, ct);

            await new WaitMasterKeyProc(device).Run(SdkConfig.SystemStateEventWaitTimeout, ct);

            await _ui.SendNotification("", _infNid);
        }

        async Task<bool> ButtonWorkflow(IDevice device, int timeout, CancellationToken ct)
        {
            if (!device.AccessLevel.IsButtonRequired)
                return true;

            await _ui.SendNotification("Please press the Button on your Hideez Key", _infNid);
            await _ui.ShowButtonConfirmUi(device.Id);
            var res = await device.WaitButtonConfirmation(timeout, ct);
            return res;
        }

        Task<bool> PinWorkflow(IDevice device, int timeout, CancellationToken ct)
        {
            if (device.AccessLevel.IsPinRequired && device.AccessLevel.IsNewPinRequired)
            {
                return SetPinWorkflow(device, timeout, ct);
            }
            else if (device.AccessLevel.IsPinRequired)
            {
                return EnterPinWorkflow(device, timeout, ct);
            }

            return Task.FromResult(true);
        }

        async Task<bool> SetPinWorkflow(IDevice device, int timeout, CancellationToken ct)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> SetPinWorkflow +++++++++++++++++++++++++++++++++++++++");

            bool res = false;
            while (device.AccessLevel.IsNewPinRequired)
            {
                await _ui.SendNotification($"Please create new PIN code for your Hideez Key (minimum {device.PinAttemptsRemain})", _infNid);
                string pin = await _ui.GetPin(device.Id, timeout, ct, withConfirm: true);

                if (string.IsNullOrWhiteSpace(pin))
                {
                    // we received an empty PIN from the user. Trying again with the same timeout.
                    WriteLine("Received empty PIN");
                    continue;
                }

                var pinResult = await device.SetPin(pin); //this using default timeout for BLE commands
                if (pinResult == HideezErrorCode.Ok)
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> PIN OK");
                    res = true;
                    break;
                }
                else if (pinResult == HideezErrorCode.ERR_PIN_TOO_SHORT)
                {
                    await _ui.SendError($"PIN too short", _errNid);
                }
                else if (pinResult == HideezErrorCode.ERR_PIN_WRONG)
                {
                    await _ui.SendError($"Invalid PIN", _errNid);
                }
            }
            Debug.WriteLine(">>>>>>>>>>>>>>> SetPinWorkflow ---------------------------------------");
            return res;
        }

        async Task<bool> EnterPinWorkflow(IDevice device, int timeout, CancellationToken ct)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> EnterPinWorkflow +++++++++++++++++++++++++++++++++++++++");

            bool res = false;
            while (!device.AccessLevel.IsLocked)
            {
                await _ui.SendNotification("Please enter the PIN code for your Hideez Key", _infNid);
                string pin = await _ui.GetPin(device.Id, timeout, ct);

                Debug.WriteLine($">>>>>>>>>>>>>>> PIN: {pin}");
                if (string.IsNullOrWhiteSpace(pin))
                {
                    // we received an empty PIN from the user. Trying again with the same timeout.
                    Debug.WriteLine($">>>>>>>>>>>>>>> EMPTY PIN");
                    WriteLine("Received empty PIN");

                    continue;
                }

                var attemptsLeft = device.PinAttemptsRemain - 1;
                var pinResult = await device.EnterPin(pin); //this using default timeout for BLE commands

                if (pinResult == HideezErrorCode.Ok)
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> PIN OK");
                    res = true;
                    break;
                }
                else if (pinResult == HideezErrorCode.ERR_DEVICE_LOCKED)
                {
                    throw new HideezException(HideezErrorCode.DeviceIsLocked);
                }
                else // ERR_PIN_WRONG and ERR_PIN_TOO_SHORT should just be displayed as wrong pin for security reasons
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> Wrong PIN ({attemptsLeft} attempts left)");
                    if (device.AccessLevel.IsLocked)
                    {
                        await _ui.SendNotification("", _infNid);
                        await _ui.SendError($"Device is locked", _errNid);
                    }
                    else
                    {
                        if (attemptsLeft > 1)
                            await _ui.SendError($"Invalid PIN! {attemptsLeft} attempts left.", _errNid);
                        else
                            await _ui.SendError($"Invalid PIN! 1 attempt left.", _errNid);
                        await device.RefreshDeviceInfo(); // Remaining pin attempts update is not quick enough 
                    }
                }
            }
            Debug.WriteLine(">>>>>>>>>>>>>>> PinWorkflow ------------------------------");
            return res;
        }

        async Task<IDevice> ConnectDevice(string mac, bool rebondOnFail, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            await _ui.SendNotification("Connecting to the device...", _infNid);

            var device = await _deviceManager.ConnectDevice(mac, SdkConfig.ConnectDeviceTimeout);

            if (device == null)
            {
                ct.ThrowIfCancellationRequested();
                await _ui.SendNotification("Connection failed. Retrying...", _infNid);

                device = await _deviceManager.ConnectDevice(mac, SdkConfig.ConnectDeviceTimeout / 2);

                if (device == null && rebondOnFail)
                {
                    ct.ThrowIfCancellationRequested();

                    // remove the bond and try one more time
                    await _deviceManager.RemoveByMac(mac);
                    await _ui.SendNotification("Connection failed. Trying re-bond the device...", _infNid);
                    device = await _deviceManager.ConnectDevice(mac, SdkConfig.ConnectDeviceTimeout);
                }
            }

            if (device == null)
                throw new Exception($"Failed to connect device '{mac}'.");

            return device;
        }

        async Task WaitDeviceInitialization(string mac, IDevice device, CancellationToken ct)
        {
            await _ui.SendNotification("Waiting for the device initialization...", _infNid);

            if (!await device.WaitInitialization(SdkConfig.DeviceInitializationTimeout, ct))
                throw new Exception($"Failed to initialize device connection '{mac}'. Please try again.");

            if (device.IsErrorState)
            {
                await _deviceManager.Remove(device);
                throw new Exception($"Failed to initialize device connection '{mac}' ({device.ErrorMessage}). Please try again.");
            }
        }

        async Task<Credentials> GetCredentials(IDevice device)
        {
            ushort primaryAccountKey = await DevicePasswordManager.GetPrimaryAccountKey(device);
            var credentials = await GetCredentials(device, primaryAccountKey);
            return credentials;
        }

        async Task<Credentials> GetCredentials(IDevice device, ushort key)
        {
            Credentials credentials;

            if (key == 0)
            {
                var str = await device.ReadStorageAsString(
                    (byte)StorageTable.BondVirtualTable1,
                    (ushort)BondVirtualTable1Item.PcUnlockCredentials);

                if (str != null)
                {
                    var parts = str.Split('\n');
                    if (parts.Length >= 2)
                    {
                        credentials.Login = parts[0];
                        credentials.Password = parts[1];
                    }
                    if (parts.Length >= 3)
                    {
                        credentials.PreviousPassword = parts[2];
                    }
                }

                if (credentials.IsEmpty)
                    throw new Exception($"Device '{device.SerialNo}' doesn't have any stored windows accounts");
            }
            else
            {
                // get the account name, login and password from the Hideez Key
                credentials.Name = await device.ReadStorageAsString((byte)StorageTable.Accounts, key);
                credentials.Login = await device.ReadStorageAsString((byte)StorageTable.Logins, key);
                credentials.Password = await device.ReadStorageAsString((byte)StorageTable.Passwords, key);
                credentials.PreviousPassword = ""; //todo

                if (credentials.IsEmpty)
                    throw new Exception($"Cannot read login or password from the device '{device.SerialNo}'");
            }

            return credentials;
        }

        async Task LicenseWorkflow(IDevice device, CancellationToken ct)
        {
            var licenses = await _hesConnection.GetNewDeviceLicenses(device.SerialNo);

            WriteLine($"Received {licenses.Count} licenses from HES");

            if (ct.IsCancellationRequested)
                return;

            foreach (var license in licenses)
            {
                if (ct.IsCancellationRequested)
                    return;

                if (license.Data == null)
                    throw new Exception($"Invalid license received from HES for {device.SerialNo}, (EMPTY_DATA). Please, contact your administrator.");

                if (license.Id == null)
                    throw new Exception($"Invalid license received from HES for {device.SerialNo}, (EMPTY_ID). Please, contact your administrator.");

                await device.LoadLicense(license.Data, SdkConfig.DefaultCommandTimeout);
                WriteLine($"Loaded license ({license.Id}) into device ({device.SerialNo})");
                await _hesConnection.OnDeviceLicenseApplied(device.SerialNo, license.Id);
            }
        }

        #region Device owner mame and owner email handling
        void CacheDeviceInfoAsync(DeviceInfoDto dto)
        {
            if (_localDeviceInfoCache != null)
            {
                Task.Run(() =>
                {
                    var info = new LocalDeviceInfo
                    {
                        Mac = dto.DeviceMac,
                        SerialNo = dto.DeviceSerialNo,
                        OwnerName = dto.OwnerName,
                        OwnerEmail = dto.OwnerEmail,
                    };

                    _localDeviceInfoCache.SaveLocalInfo(info);
                }).ConfigureAwait(false);
            }
            else
                WriteLine("Failed to cache info: Local info cache not available");
        }

        void CacheAndUpdateDeviceOwner(IDevice device, DeviceInfoDto dto)
        {
            UpdateDeviceOwner(device, dto.OwnerName, dto.OwnerEmail);
            CacheDeviceInfoAsync(dto);
        }

        void LoadLocalDeviceOwner(IDevice device)
        {
            if (_localDeviceInfoCache != null)
            {
                var localDeviceInfo = _localDeviceInfoCache.GetLocalInfo(device.Mac);
                if (localDeviceInfo != null)
                    UpdateDeviceOwner(device, localDeviceInfo.OwnerName, localDeviceInfo.OwnerEmail);
            }
            else
                WriteLine("Failed to load info: Local info cache not available");
        }

        void UpdateDeviceOwner(IDevice device, string ownerName, string ownerEmail)
        {
            if (!string.IsNullOrWhiteSpace(ownerName))
                device.SetUserProperty(OWNER_NAME_PROP, ownerName);

            if (!string.IsNullOrWhiteSpace(ownerEmail))
                device.SetUserProperty(OWNER_EMAIL_PROP, ownerEmail);
        }
        #endregion
    }
}
