using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using HideezClient.HideezServiceReference;
using HideezClient.Messages;
using HideezClient.Modules;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using HideezClient.Utilities;
using HideezMiddleware;
using Microsoft.Win32;
using MvvmExtensions.Attributes;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hideez.SDK.Communication.Log;
using HideezClient.Modules.Log;

namespace HideezClient.Models
{
    // Todo: Implement thread-safety lock for password manager and remote device
    public class Device : ObservableObject, IDisposable
    {
        const int INIT_TIMEOUT = 5_000;
        readonly int CREDENTIAL_TIMEOUT = SdkConfig.MainWorkflowTimeout;

        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(Device));
        readonly IServiceProxy _serviceProxy;
        readonly IRemoteDeviceFactory _remoteDeviceFactory;
        readonly IMessenger _messenger;
        RemoteDevice _remoteDevice;
        DelayedMethodCaller dmc = new DelayedMethodCaller(2000);
        readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _pendingGetPinRequests
            = new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();

        string _infNid = Guid.NewGuid().ToString(); // Notification Id, which must be the same for the entire duration of MainWorkflow
        string _errNid = Guid.NewGuid().ToString(); // Error Notification Id

        string id;
        string name;
        string ownerName;
        string ownerEmail;
        bool isConnected;
        bool isInitialized;
        string serialNo;
        uint storageTotalSize;
        uint storageFreeSize;
        Version firmwareVersion;
        Version bootloaderVersion;
        int pinAttemptsRemain;
        double proximity = 0;
        int battery = 0;
        bool finishedMainFlow;
        byte storageUpdateCounter;

        bool isCreatingRemoteDevice;
        bool isAuthorizingRemoteDevice;
        bool isLoadingStorage;
        bool isStorageLoaded;
        bool isProximityLockEnabled;

        CancellationTokenSource authCancellationTokenSource;
        int _interlockedRemote = 0;

        public Device(
            IServiceProxy serviceProxy,
            IRemoteDeviceFactory remoteDeviceFactory,
            IMessenger messenger,
            DeviceDTO dto)
        {
            _serviceProxy = serviceProxy;
            _remoteDeviceFactory = remoteDeviceFactory;
            _messenger = messenger;

            PropertyChanged += Device_PropertyChanged;

            _messenger.Register<DeviceConnectionStateChangedMessage>(this, OnDeviceConnectionStateChanged);
            _messenger.Register<DeviceInitializedMessage>(this, OnDeviceInitialized);
            _messenger.Register<DeviceFinishedMainFlowMessage>(this, OnDeviceFinishedMainFlow);
            _messenger.Register<SendPinMessage>(this, OnPinReceived);
            _messenger.Register<DeviceOperationCancelledMessage>(this, OnOperationCancelled);
            _messenger.Register<DeviceProximityChangedMessage>(this, OnDeviceProximityChanged);
            _messenger.Register<DeviceBatteryChangedMessage>(this, OnDeviceBatteryChanged);
            _messenger.Register<SessionSwitchMessage>(this, OnSessionSwitch);
            _messenger.Register<DeviceProximityLockEnabledMessage>(this, OnDeviceProximityLockEnabled);

            RegisterDependencies();

            LoadFrom(dto);
        }

        #region Properties
        public IDeviceStorage Storage
        {
            get
            {
                return _remoteDevice;
            }
        }

        public DevicePasswordManager PasswordManager { get; private set; }

        public string TypeName { get; } = "Hideez key";

        // There properties are set by constructor and updated by certain events and messages
        public string Id
        {
            get { return id; }
            private set { Set(ref id, value); }
        }

        public string Name
        {
            get { return name; }
            private set { Set(ref name, value); }
        }

        public string OwnerName
        {
            get { return ownerName; }
            private set { Set(ref ownerName, value); }
        }

        public string OwnerEmail
        {
            get { return ownerEmail; }
            private set { Set(ref ownerEmail, value); }
        }

        public bool IsConnected
        {
            get { return isConnected; }
            private set { Set(ref isConnected, value); }
        }

        // TODO: This property is no longer required
        public bool IsInitialized
        {
            get { return isInitialized; }
            set { Set(ref isInitialized, value); }
        }

        public string SerialNo
        {
            get { return serialNo; }
            private set { Set(ref serialNo, value); }
        }

        public Version FirmwareVersion
        {
            get { return firmwareVersion; }
            private set { Set(ref firmwareVersion, value); }
        }

        public Version BootloaderVersion
        {
            get { return bootloaderVersion; }
            private set { Set(ref bootloaderVersion, value); }
        }

        public uint StorageTotalSize
        {
            get { return storageTotalSize; }
            private set { Set(ref storageTotalSize, value); }
        }

        public uint StorageFreeSize
        {
            get { return storageFreeSize; }
            private set { Set(ref storageFreeSize, value); }
        }

        [DependsOn(nameof(IsConnected), nameof(IsInitialized), nameof(FinishedMainFlow))]
        public bool IsInitializing
        {
            get { return IsConnected && (!IsInitialized || !FinishedMainFlow); }
        }

        [DependsOn(nameof(IsConnected))]
        public double Proximity
        {
            get { return IsConnected ? proximity : 0; }
            set { Set(ref proximity, value); }
        }

        [DependsOn(nameof(IsConnected))]
        public int Battery
        {
            get { return IsConnected ? battery : 0; }
            set { Set(ref battery, value); }
        }

        public bool FinishedMainFlow
        {
            get { return finishedMainFlow; }
            set { Set(ref finishedMainFlow, value); }
        }

        // These properties depend upon some internal processes 
        public bool IsCreatingRemoteDevice
        {
            get { return isCreatingRemoteDevice; }
            set { Set(ref isCreatingRemoteDevice, value); }
        }
        
        public bool IsAuthorizingRemoteDevice
        {
            get { return isAuthorizingRemoteDevice; }
            private set { Set(ref isAuthorizingRemoteDevice, value); }
        }

        public bool IsLoadingStorage
        {
            get { return isLoadingStorage; }
            private set { Set(ref isLoadingStorage, value); }
        }

        public bool IsStorageLoaded
        {
            get { return isStorageLoaded; }
            private set { Set(ref isStorageLoaded, value); }
        }


        // These properties are tied to the RemoteDevice 
        [DependsOn(nameof(IsConnected))]
        public AccessLevel AccessLevel
        {
            get { return _remoteDevice?.AccessLevel; }
        }

        public int PinAttemptsRemain
        {
            get { return _remoteDevice != null ? (int)_remoteDevice?.PinAttemptsRemain : 0; }
        }

        [DependsOn(nameof(AccessLevel))]
        public bool IsAuthorized
        {
            get
            {
                if (_remoteDevice == null)
                    return false;

                if (_remoteDevice.AccessLevel == null)
                    return false;

                return _remoteDevice.AccessLevel.IsAllOk;
            }
        }

        public bool CanLockByProximity
        {
            get { return isProximityLockEnabled; }
            private set { Set(ref isProximityLockEnabled, value); }
        }
        #endregion

        #region Messege & Event handlers
        void RemoteDevice_ButtonPressed(object sender, Hideez.SDK.Communication.ButtonPressCode e)
        {
            _log.WriteLine($"Device ({Id}) button pressed, code: {e}");

            Task.Run(() =>
            {
                _messenger.Send(new DeviceButtonPressedMessage(Id, e));
            });
        }

        void RemoteDevice_StorageModified(object sender, EventArgs e)
        {
            _log.WriteLine($"Device ({Id}) storage modified");

            Task.Run(() =>
            {
                dmc.CallMethod(async () =>
                {
                    if (_remoteDevice != null && PasswordManager != null)
                    {
                        var updateCounter = _remoteDevice.StorageUpdateCounter;
                        var loadedUpdateCounter = PasswordManager.LoadedStorageUpdateCounter;
                        var delta = loadedUpdateCounter - updateCounter;
                        if (updateCounter > loadedUpdateCounter || delta > 100)
                        {
                            try
                            {
                                await LoadStorage();
                            }
                            catch (Exception ex)
                            {
                                _log.WriteLine(ex);
                            }
                        }
                    }

                                
                });
            });

        }

        void RemoteDevice_PropertyChanged(object sender, string e)
        {
            switch (e)
            {
                case nameof(AccessLevel):
                case nameof(PinAttemptsRemain):
                    RaisePropertyChanged(e);
                    break;
                default:
                    break;
            }
        }

        async void OnDeviceConnectionStateChanged(DeviceConnectionStateChangedMessage obj)
        {
            if (obj.Device.Id == Id)
            {
                LoadFrom(obj.Device);

                if (!obj.Device.IsConnected)
                    await ShutdownRemoteDeviceAsync(HideezErrorCode.DeviceDisconnected);
            }
        }

        void OnDeviceInitialized(DeviceInitializedMessage obj)
        {
            if (obj.Device.Id == Id)
                LoadFrom(obj.Device);
        }

        void OnDeviceFinishedMainFlow(DeviceFinishedMainFlowMessage obj)
        {
            if (obj.Device.Id == Id)
                LoadFrom(obj.Device);
        }

        void OnPinReceived(SendPinMessage obj)
        {
            if (_pendingGetPinRequests.TryGetValue(obj.DeviceId, out TaskCompletionSource<byte[]> tcs))
                tcs.TrySetResult(obj.Pin);
        }

        void OnSessionSwitch(SessionSwitchMessage obj)
        {
            switch (obj.Reason)
            {
                case SessionSwitchReason.SessionLogoff:
                case SessionSwitchReason.SessionLock:
                    // Workstation lock should cancel ongoing remote device authorization
                    CancelDeviceAuthorization();
                    break;
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                    // Workstation unlock is one of reasons to try create remote device
                    TryInitRemoteAsync();
                    break;
                default:
                    return;
            }
        }

        void OnOperationCancelled(DeviceOperationCancelledMessage obj)
        {
            if (obj.Device.Id == Id)
            {
                CancelDeviceAuthorization();
            }
        }

        void OnDeviceProximityChanged(DeviceProximityChangedMessage obj)
        {
            // Todo: MAYBE it will be beneficial to add a check that device is connected
            if (Id != obj.DeviceId)
                return;

            Proximity = obj.Proximity;
        }

        void OnDeviceBatteryChanged(DeviceBatteryChangedMessage obj)
        {
            if (Id != obj.DeviceId)
                return;

            Battery = obj.Battery;
        }

        void OnDeviceProximityLockEnabled(DeviceProximityLockEnabledMessage obj)
        {
            if (obj.Device.Id == Id)
                LoadFrom(obj.Device);
        }

        void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FinishedMainFlow))
            {
                TryInitRemoteAsync();
            }
        }

        #endregion

        void LoadFrom(DeviceDTO dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            OwnerName = dto.OwnerName;
            OwnerEmail = dto.OwnerEmail;
            IsConnected = dto.IsConnected;
            IsInitialized = dto.IsInitialized;
            SerialNo = dto.SerialNo;
            FirmwareVersion = dto.FirmwareVersion;
            BootloaderVersion = dto.BootloaderVersion;
            StorageTotalSize = dto.StorageTotalSize;
            StorageFreeSize = dto.StorageFreeSize;
            Proximity = dto.Proximity;
            Battery = dto.Battery;
            FinishedMainFlow = dto.FinishedMainFlow;
            CanLockByProximity = dto.CanLockPyProximity;
        }

        async void TryInitRemoteAsync()
        {
            if (FinishedMainFlow)
            {
                try
                {
                    await InitRemoteAndLoadStorageAsync(false);
                }
                catch (Exception ex)
                {
                    _log.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// Create and authorize remote device, then load credentials storage from this device
        /// </summary>
        /// <param name="authorizeDevice">If false, skip remote device authorization step. Default is true.</param>
        public async Task InitRemoteAndLoadStorageAsync(bool authorizeDevice = true)
        {
            if (Interlocked.CompareExchange(ref _interlockedRemote, 1, 0) == 0)
            {
                try
                {
                    if (!IsCreatingRemoteDevice && 
                        !IsAuthorizingRemoteDevice && 
                        !IsLoadingStorage && 
                        WorkstationHelper.GetCurrentSessionLockState() == WorkstationHelper.LockState.Unlocked)
                    {
                        try
                        {
                            authCancellationTokenSource = new CancellationTokenSource();
                            var ct = authCancellationTokenSource.Token;

                            _infNid = Guid.NewGuid().ToString();
                            _errNid = Guid.NewGuid().ToString();

                            await CreateRemoteDeviceAsync();

                            if (authorizeDevice && !IsAuthorized && _remoteDevice?.AccessLevel != null && !_remoteDevice.AccessLevel.IsAllOk)
                                await AuthorizeRemoteDevice(ct);
                            else if (!authorizeDevice && !IsAuthorized && _remoteDevice?.AccessLevel != null && _remoteDevice.AccessLevel.IsAllOk)
                                await AuthorizeRemoteDevice(ct);

                            if (!ct.IsCancellationRequested && !IsStorageLoaded)
                                await LoadStorage();
                        }
                        catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.DeviceDisconnected)
                        {
                            _log.WriteLine("Remote device creation aborted, device disconnected", LogErrorSeverity.Warning);
                        }
                        catch (Exception ex)
                        {
                            ShowError(ex.Message, _errNid);
                        }
                        finally
                        {
                            authCancellationTokenSource.Dispose();
                            authCancellationTokenSource = null;
                        }
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _interlockedRemote, 0);
                }
            }
        }

        public void CancelDeviceAuthorization()
        {
            authCancellationTokenSource?.Cancel();
        }

        public async Task ShutdownRemoteDeviceAsync(HideezErrorCode code)
        {
            try
            {
                if (_remoteDevice != null)
                {
                    CancelDeviceAuthorization();

                    var tempRemoteDevice = _remoteDevice;
                    _remoteDevice = null;
                    PasswordManager = null;

                    tempRemoteDevice.ButtonPressed -= RemoteDevice_ButtonPressed;
                    tempRemoteDevice.StorageModified -= RemoteDevice_StorageModified;
                    tempRemoteDevice.PropertyChanged -= RemoteDevice_PropertyChanged;
                    await tempRemoteDevice.Shutdown(code);
                    await _serviceProxy.GetService().RemoveDeviceAsync(tempRemoteDevice.Id);
                }
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
                
            IsCreatingRemoteDevice = false;
            IsAuthorizingRemoteDevice = false;
            IsLoadingStorage = false;
            IsStorageLoaded = false;

            NotifyPropertyChanged(nameof(IsAuthorized));
        }

        async Task CreateRemoteDeviceAsync()
        {
            if (_remoteDevice != null || IsCreatingRemoteDevice)
                return;

            HideezErrorCode initErrorCode = HideezErrorCode.Ok;

            try
            {
                _log.WriteLine($"Device ({SerialNo}), establishing remote device connection");

                _log.WriteLine("Checking for available channels");
                var channels = await _serviceProxy.GetService().GetAvailableChannelsAsync(SerialNo);
                if (channels.Length == 0)
                    throw new Exception($"No available channels on device ({SerialNo})"); // Todo: separate exception type
                var channelNo = channels.FirstOrDefault();
                _log.WriteLine($"{channels.Length} channels available");

                ShowInfo($"Preparing for device ({SerialNo}) authorization", _infNid);
                IsCreatingRemoteDevice = true;
                _remoteDevice = await _remoteDeviceFactory.CreateRemoteDeviceAsync(SerialNo, channelNo);
                _remoteDevice.PropertyChanged += RemoteDevice_PropertyChanged;

                await _remoteDevice.Verify();
                await _remoteDevice.Initialize(INIT_TIMEOUT);

                if (_remoteDevice.SerialNo != SerialNo)
                    throw new Exception("Remote device serial number does not match the enumerated serial");

                _log.WriteLine($"Creating password manager for device ({SerialNo})");
                PasswordManager = new DevicePasswordManager(_remoteDevice, null);
                _remoteDevice.StorageModified += RemoteDevice_StorageModified;
                _remoteDevice.ButtonPressed += RemoteDevice_ButtonPressed;

                _log.WriteLine($"Remote device ({SerialNo}) connection established");
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                _log.WriteLine(ex.FormattedMessage());
                ShowError(ex.FormattedMessage(), _errNid);
                initErrorCode = (HideezErrorCode)ex.Detail.ErrorCode;
            }
            catch (HideezException ex)
            {
                _log.WriteLine(ex);
                ShowError(ex.Message, _errNid);
                initErrorCode = ex.ErrorCode;
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
                ShowError(ex.Message, _errNid);
                initErrorCode = HideezErrorCode.UnknownError;
            }
            finally
            {
                ShowInfo("", _infNid);
                _messenger.Send(new HidePinUiMessage());

                if (initErrorCode != HideezErrorCode.Ok)
                    await ShutdownRemoteDeviceAsync(initErrorCode);

                IsCreatingRemoteDevice = false;
            }

        }

        async Task AuthorizeRemoteDevice(CancellationToken ct)
        {
            if (_remoteDevice == null || IsAuthorized || IsAuthorizingRemoteDevice)
                return;

            try
            {
                IsAuthorizingRemoteDevice = true;

                if (_remoteDevice.AccessLevel.IsLocked)
                    throw new HideezException(HideezErrorCode.DeviceIsLocked);

                else if (_remoteDevice.AccessLevel.IsLinkRequired)
                    throw new HideezException(HideezErrorCode.DeviceNotAssignedToUser);

                await ButtonWorkflow(ct);
                await PinWorkflow(ct);

                if (ct.IsCancellationRequested)
                    ShowError($"Authorization cancelled for device ({SerialNo})", _errNid);
                else if (IsAuthorized)
                    _log.WriteLine($"Remote device ({_remoteDevice.Id}) is authorized");
                else
                    ShowError($"Authorization for device ({SerialNo}) failed", _errNid);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                _log.WriteLine(ex.FormattedMessage());
                ShowError(ex.FormattedMessage(), _errNid);
            }
            catch (HideezException ex)
            {
                _log.WriteLine(ex);
                ShowError(ex.Message, _errNid);
            }
            catch (OperationCanceledException ex)
            {
                _log.WriteLine(ex);
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
                ShowError(ex.Message, _errNid);
            }
            finally
            {
                ShowInfo("", _infNid);
                _messenger.Send(new HidePinUiMessage());

                if (IsAuthorized)
                    ShowError("", _errNid);

                IsAuthorizingRemoteDevice = false;
            }
        }

        async Task LoadStorage()
        {
            if (_remoteDevice == null || !IsAuthorized || IsLoadingStorage || PasswordManager == null)
                return;

            try
            {
                IsStorageLoaded = false;
                IsLoadingStorage = true;

                _log.WriteLine($"Device ({Id}) loading storage");
                await PasswordManager.Load();
                _log.WriteLine($"Device ({Id}) loaded {PasswordManager.Accounts.Count} entries from storage");
                
                IsStorageLoaded = true;
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                _log.WriteLine(ex);
                ShowError(ex.Message, _errNid);

                await ShutdownRemoteDeviceAsync(HideezErrorCode.UnknownError);
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
                ShowError(ex.Message, _errNid);

                await ShutdownRemoteDeviceAsync(HideezErrorCode.UnknownError);
            }
            finally
            {
                IsLoadingStorage = false;
            }
        }


        async Task<bool> ButtonWorkflow(CancellationToken ct)
        {
            if (!_remoteDevice.AccessLevel.IsButtonRequired)
                return true;

            ShowInfo("Please press the Button on your Hideez Key", _infNid);
            _messenger.Send(new ShowButtonConfirmUiMessage(Id));
            var res = await _remoteDevice.WaitButtonConfirmation(CREDENTIAL_TIMEOUT, ct);
            return res;
        }

        Task<bool> PinWorkflow(CancellationToken ct)
        {
            if (_remoteDevice.AccessLevel.IsPinRequired && _remoteDevice.AccessLevel.IsNewPinRequired)
            {
                return SetPinWorkflow(ct);
            }
            else if (_remoteDevice.AccessLevel.IsPinRequired)
            {
                return EnterPinWorkflow(ct);
            }

            return Task.FromResult(true);
        }

        async Task<bool> SetPinWorkflow(CancellationToken ct)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> SetPinWorkflow +++++++++++++++++++++++++++++++++++++++");

            bool pinOk = false;
            while (AccessLevel.IsNewPinRequired)
            {
                ShowInfo("Please create new PIN code for your Hideez Key", _infNid);
                var pin = await GetPin(Id, CREDENTIAL_TIMEOUT, ct, withConfirm: true);

                if (pin == null)
                    return false; // finished by timeout from the _ui.GetPin

                if (pin.Length == 0)
                {
                    // we received an empty PIN from the user. Trying again with the same timeout.
                    Debug.WriteLine($">>>>>>>>>>>>>>> EMPTY PIN");
                    _log.WriteLine("Received empty PIN");
                    continue;
                }

                var pinResult = await _remoteDevice.SetPin(Encoding.UTF8.GetString(pin)); //this using default timeout for BLE commands
                if (pinResult == HideezErrorCode.Ok)
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> PIN OK");
                    pinOk = true;
                    break;
                }
                else if (pinResult == HideezErrorCode.ERR_PIN_TOO_SHORT)
                {
                    ShowError("PIN is too short", _errNid);
                }
                else if (pinResult == HideezErrorCode.ERR_PIN_WRONG)
                {
                    ShowError("Invalid PIN", _errNid);
                }
            }
            Debug.WriteLine(">>>>>>>>>>>>>>> SetPinWorkflow ---------------------------------------");
            return pinOk;
        }

        async Task<bool> EnterPinWorkflow(CancellationToken ct)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> EnterPinWorkflow +++++++++++++++++++++++++++++++++++++++");

            bool pinOk = false;
            while (!AccessLevel.IsLocked)
            {
                ShowInfo("Please enter the PIN code for your Hideez Key", _infNid);
                var pin = await GetPin(Id, CREDENTIAL_TIMEOUT, ct);

                if (pin == null)
                    return false; // finished by timeout from the _ui.GetPin

                if (pin.Length == 0)
                {
                    // we received an empty PIN from the user. Trying again with the same timeout.
                    Debug.WriteLine($">>>>>>>>>>>>>>> EMPTY PIN");
                    _log.WriteLine("Received empty PIN");

                    continue;
                }

                var pinResult = await _remoteDevice.EnterPin(Encoding.UTF8.GetString(pin)); //this using default timeout for BLE commands

                if (pinResult == HideezErrorCode.Ok)
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> PIN OK");
                    pinOk = true;
                    break;
                }
                else // ERR_PIN_WRONG and ERR_PIN_TOO_SHORT should just be displayed as wrong pin for security reasons
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> Wrong PIN ({PinAttemptsRemain} attempts left)");
                    if (AccessLevel.IsLocked)
                        ShowError($"Device is locked", _errNid);
                    else
                        ShowError($"Wrong PIN ({PinAttemptsRemain} attempts left)", _errNid);
                }
            }
            Debug.WriteLine(">>>>>>>>>>>>>>> PinWorkflow ------------------------------");
            return pinOk;
        }

        async Task<byte[]> GetPin(string deviceId, int timeout, CancellationToken ct, bool withConfirm = false, bool askOldPin = false)
        {
            _messenger.Send(new ShowPinUiMessage(deviceId, withConfirm, askOldPin));

            var tcs = _pendingGetPinRequests.GetOrAdd(deviceId, (x) =>
            {
                return new TaskCompletionSource<byte[]>();
            });

            try
            {
                return await tcs.Task.TimeoutAfter(timeout, ct);
            }
            catch (TimeoutException)
            {
                return null;
            }
            finally
            {
                _pendingGetPinRequests.TryRemove(deviceId, out TaskCompletionSource<byte[]> removed);
            }
        }


        #region Notifications display
        void ShowInfo(string message, string notificationId)
        {
            _messenger?.Send(new ShowInfoNotificationMessage(message, notificationId: notificationId));
        }

        void ShowError(string message, string notificationId)
        {
            _messenger?.Send(new ShowErrorNotificationMessage(message, notificationId: notificationId));
        }

        void ShowWarn(string message, string notificationId)
        {
            _messenger?.Send(new ShowWarningNotificationMessage(message, notificationId: notificationId));
        }
        #endregion

        #region IDisposable Support
        bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    PropertyChanged -= Device_PropertyChanged;
                    _messenger.Unregister(this);
                }

                disposed = true;
            }
        }

        ~Device()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
