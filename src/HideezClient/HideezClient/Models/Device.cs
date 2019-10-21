using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
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
using Microsoft.Win32;
using MvvmExtensions.Attributes;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HideezClient.Models
{
    // Todo: Implement thread-safety lock for password manager and remote device
    public class Device : ObservableObject, IDisposable
    {
        const int VERIFY_CHANNEL = 2;
        const int INIT_WAIT = 5_000;
        const int RETRY_DELAY = 2_500;
        readonly int CREDENTIAL_TIMEOUT = SdkConfig.MainWorkflowTimeout;

        readonly ILogger _log = LogManager.GetCurrentClassLogger();
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
        bool isConnected;
        string serialNo;
        uint storageTotalSize;
        uint storageFreeSize;
        Version firmwareVersion;
        Version bootloaderVersion;
        bool isInitializing;
        bool isAuthorizing;
        bool isLoadingStorage;
        bool isStorageLoaded;
        int pinAttemptsRemain;

        CancellationTokenSource authCancellationTokenSource;

        public Device(
            IServiceProxy serviceProxy,
            IRemoteDeviceFactory remoteDeviceFactory,
            IMessenger messenger,
            DeviceDTO dto)
        {
            _serviceProxy = serviceProxy;
            _remoteDeviceFactory = remoteDeviceFactory;
            _messenger = messenger;

            SystemEvents.SessionSwitch += OnSessionSwitch;

            _messenger.Register<DeviceConnectionStateChangedMessage>(this, OnDeviceConnectionStateChanged);
            _messenger.Register<DeviceInitializedMessage>(this, OnDeviceInitialized);
            _messenger.Register<SendPinMessage>(this, OnPinReceived);
            _messenger.Register<DeviceOperationCancelledMessage>(this, OnOperationCancelled);

            RegisterDependencies();

            LoadFrom(dto);
        }

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

        public bool IsConnected
        {
            get { return isConnected; }
            private set
            {
                Set(ref isConnected, value);
                if (!isConnected)
                {
                    CloseRemoteDeviceConnection();
                    CancelDeviceAuthorization();
                }
            }
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

        // These properties depend upon some processes internal
        public bool IsInitializing
        {
            get { return isInitializing; }
            private set { Set(ref isInitializing, value); }
        }

        public bool IsAuthorizing
        {
            get { return isAuthorizing; }
            private set { Set(ref isAuthorizing, value); }
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
        public double Proximity
        {
            get { return _remoteDevice != null ? _remoteDevice.Proximity : 0; }
        }

        public int Battery
        {
            get { return _remoteDevice != null ? _remoteDevice.Battery : 0; }
        }

        public AccessLevel AccessLevel
        {
            get { return _remoteDevice?.AccessLevel; }
        }

        public int PinAttemptsRemain
        {
            get { return _remoteDevice != null ? (int)_remoteDevice?.PinAttemptsRemain : 0; }
        }

        public bool IsInitialized
        {
            get { return _remoteDevice != null ? _remoteDevice.IsInitialized : false; }
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


        void RemoteDevice_StorageModified(object sender, EventArgs e)
        {
            _log.Info($"Device ({SerialNo}) storage modified");
            if (!IsAuthorized || IsLoadingStorage)
                return;

            Task.Run(() =>
            {
                dmc.CallMethod(async () =>
                {
                    try
                    {
                        await LoadStorage();
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex);
                    }
                });
            });

        }

        void RemoteDevice_PropertyChanged(object sender, string e)
        {
            RaisePropertyChanged(e);
        }

        void OnDeviceConnectionStateChanged(DeviceConnectionStateChangedMessage obj)
        {
            if (obj.Device.Id == Id)
                LoadFrom(obj.Device);
        }

        void OnDeviceInitialized(DeviceInitializedMessage obj)
        {
            if (obj.Device.Id == Id)
                LoadFrom(obj.Device);
        }

        void OnPinReceived(SendPinMessage obj)
        {
            if (_pendingGetPinRequests.TryGetValue(obj.DeviceId, out TaskCompletionSource<byte[]> tcs))
                tcs.TrySetResult(obj.Pin);
        }

        void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            CancelDeviceAuthorization();
        }

        void OnOperationCancelled(DeviceOperationCancelledMessage obj)
        {
            if (obj.Device.Id == Id)
            {
                CancelDeviceAuthorization();
            }
        }

        void LoadFrom(DeviceDTO dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            OwnerName = dto.Owner ?? "...unspecified...";
            IsConnected = dto.IsConnected;
            SerialNo = dto.SerialNo;
            FirmwareVersion = dto.FirmwareVersion;
            BootloaderVersion = dto.BootloaderVersion;
            StorageTotalSize = dto.StorageTotalSize;
            StorageFreeSize = dto.StorageFreeSize;
        }

        public async Task InitializeRemoteDevice()
        {
            if (IsInitialized || IsInitializing)
                return;

            IsInitializing = true;

            try
            {
                while (IsInitializing && !IsInitialized && IsConnected)
                {
                    try
                    {
                        _log.Info($"Device ({SerialNo}), establishing remote device connection");
                        _remoteDevice = await _remoteDeviceFactory.CreateRemoteDeviceAsync(SerialNo, VERIFY_CHANNEL);
                        _remoteDevice.PropertyChanged += RemoteDevice_PropertyChanged;


                        await _remoteDevice.Verify(VERIFY_CHANNEL);
                        await _remoteDevice.Initialize(INIT_WAIT);

                        if (_remoteDevice.SerialNo != SerialNo)
                        {
                            _remoteDevice.PropertyChanged -= RemoteDevice_PropertyChanged;
                            _serviceProxy.GetService().RemoveDevice(_remoteDevice.Id);
                            throw new Exception("Remote device serial number does not match the enumerated serial");
                        }

                        _remoteDevice.StorageModified += RemoteDevice_StorageModified;
                        PasswordManager = new DevicePasswordManager(_remoteDevice, null);

                        _log.Info($"Remote device ({SerialNo}) initialized");
                    }
                    catch (FaultException<HideezServiceFault> ex)
                    {
                        _log.Error(ex.FormattedMessage());
                        ShowError(ex.FormattedMessage(), _errNid);
                    }
                    catch (HideezException ex)
                    {
                        _log.Error(ex);
                        ShowError(ex.Message, _errNid);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex);
                        ShowError(ex.Message, _errNid);
                    }
                    finally
                    {
                        if (IsInitializing)
                            await Task.Delay(RETRY_DELAY);
                    }
                }
            }
            finally
            {
                IsInitializing = false;
            }
        }

        public async Task AuthorizeRemoteDevice(CancellationToken ct)
        {
            if (!IsInitialized)
                throw new HideezException(HideezErrorCode.ChannelNotInitialized); // Todo: proper exception

            if (IsAuthorized || IsAuthorizing)
                return; // Remote is already authorized

            try
            {
                IsAuthorizing = true;

                _infNid = Guid.NewGuid().ToString();
                _errNid = Guid.NewGuid().ToString();

                if (_remoteDevice.AccessLevel.IsLocked)
                    throw new HideezException(HideezErrorCode.DeviceIsLocked);

                else if (_remoteDevice.AccessLevel.IsLinkRequired)
                    throw new HideezException(HideezErrorCode.DeviceNotAssignedToUser);

                if (await ButtonWorkflow(ct) && await PinWorkflow(ct))
                {
                    // check the button again as it may be outdated while PIN workflow was running
                    await ButtonWorkflow(ct); //todo - fix FW
                }

                if (ct.IsCancellationRequested)
                    ShowError($"Authorization cancelled for device ({SerialNo})", _errNid);
                else if (IsAuthorized)
                    _log.Info($"Remote device ({_remoteDevice.Id}) is authorized");
                else
                    ShowError($"Authorization for device ({SerialNo}) failed", _errNid);
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                _log.Error(ex.FormattedMessage());
                ShowError(ex.FormattedMessage(), _errNid);
            }
            catch (HideezException ex)
            {
                _log.Error(ex);
                ShowError(ex.Message, _errNid);
            }
            catch (OperationCanceledException ex)
            {
                _log.Error(ex);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                ShowError(ex.Message, _errNid);
            }
            finally
            {
                ShowInfo("", _infNid);
                _messenger.Send(new HidePinUiMessage());

                if (IsAuthorized)
                    ShowError("", _errNid);

                IsAuthorizing = false;
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
            if (_remoteDevice.AccessLevel.IsNewPinRequired)
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
                    _log.Info("Received empty PIN");
                    continue;
                }

                pinOk = await _remoteDevice.SetPin(Encoding.UTF8.GetString(pin)); //this using default timeout for BLE commands
            }

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
                    _log.Info("Received empty PIN");

                    continue;
                }

                pinOk = await _remoteDevice.EnterPin(Encoding.UTF8.GetString(pin)); //this using default timeout for BLE commands

                if (pinOk)
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> PIN OK");
                    break;
                }
                else
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

        public async Task LoadStorage()
        {
            if (!IsInitialized)
                throw new HideezException(HideezErrorCode.ChannelNotInitialized);

            if (!IsAuthorized)
                throw new HideezException(HideezErrorCode.ChannelNotAuthorized);

            if (IsLoadingStorage)
                return;

            try
            {
                IsStorageLoaded = false;

                IsLoadingStorage = true;

                _log.Info($"Device ({SerialNo}) loading storage");

                await PasswordManager.Load();

                _log.Info($"Device ({SerialNo}) loaded {PasswordManager.Accounts.Count} entries from storage");

                IsStorageLoaded = true;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            finally
            {
                IsLoadingStorage = false;
            }
        }

        public void CloseRemoteDeviceConnection()
        {
            if (_remoteDevice != null)
            {
                _remoteDevice.StorageModified -= RemoteDevice_StorageModified;
                _remoteDevice.PropertyChanged -= RemoteDevice_PropertyChanged;
                _remoteDevice = null;
                PasswordManager = null;
            }

            IsInitializing = false;
            IsAuthorizing = false;
            IsLoadingStorage = false;
            IsStorageLoaded = false;
        }

        public void CancelDeviceAuthorization()
        {
            authCancellationTokenSource?.Cancel();
        }

        public async Task AuthorizeAndLoadStorage()
        {
            if (!IsAuthorizing && !IsLoadingStorage)
            {
                try
                {
                    authCancellationTokenSource = new CancellationTokenSource();
                    var ct = authCancellationTokenSource.Token;

                    await AuthorizeRemoteDevice(ct);

                    if (!ct.IsCancellationRequested)
                        await LoadStorage();
                }
                catch (FaultException<HideezServiceFault> ex)
                {
                    _messenger.Send(new ShowErrorNotificationMessage(ex.Message));
                }
                catch (Exception ex)
                {
                    _messenger.Send(new ShowErrorNotificationMessage(ex.Message));
                }
                finally
                {
                    authCancellationTokenSource.Dispose();
                    authCancellationTokenSource = null;
                }
            }
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

        #region IDisposable Support
        bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                }

                SystemEvents.SessionSwitch -= OnSessionSwitch;

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
