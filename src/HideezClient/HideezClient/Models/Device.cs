using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
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
    public enum PinOperation
    {
        Unknown,
        Successful,
        Canceled,
        AccessDenied,
        Error,
    }

    // Todo: Implement thread-safety lock for password manager and remote device
    public class Device : ObservableObject
    {
        const int VERIFY_CHANNEL = 2;
        const int VERIFY_WAIT = 20_000;
        const int INIT_WAIT = 5_000;
        const int RETRY_DELAY = 2_500;
        const int CREDENTIAL_TIMEOUT = 30_000;

        readonly ILogger _log = LogManager.GetCurrentClassLogger();
        readonly IServiceProxy _serviceProxy;
        readonly IRemoteDeviceFactory _remoteDeviceFactory;
        readonly IMessenger _messenger;
        RemoteDevice _remoteDevice;
        DelayedMethodCaller dmc = new DelayedMethodCaller(2000);
        readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _pendingGetPinRequests
            = new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();

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

        string faultMessage = string.Empty;

        public Device(
            IServiceProxy serviceProxy, 
            IRemoteDeviceFactory remoteDeviceFactory, 
            IMessenger messenger, 
            DeviceDTO dto)
        {
            _serviceProxy = serviceProxy;
            _remoteDeviceFactory = remoteDeviceFactory;
            _messenger = messenger;

            _messenger.Register<DeviceConnectionStateChangedMessage>(this, OnDeviceConnectionStateChanged);
            _messenger.Register<DeviceInitializedMessage>(this, OnDeviceInitialized);
            _messenger.Register<SendPinMessage>(this, OnPinReceived);

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

        [DependsOn(nameof(FaultMessage))]
        public bool IsFaulted
        {
            get
            {
                return !string.IsNullOrWhiteSpace(FaultMessage);
            }
        }

        public string FaultMessage
        {
            get
            {
                return faultMessage;
            }
            private set
            {
                Set(ref faultMessage, value);
            }
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
            get { return _remoteDevice != null ? _remoteDevice.PinAttemptsRemain : 0; }
        }

        public bool IsInitialized
        {
            get { return _remoteDevice != null ? _remoteDevice.IsInitialized : false; }
        }

        [DependsOn(nameof(AccessLevel))]
        public bool IsAuthorized
        {
            get { return _remoteDevice != null ? _remoteDevice.AccessLevel.IsAuthorized : false; }
        }


        void RemoteDevice_StorageModified(object sender, EventArgs e)
        {
            _log.Info($"Device ({SerialNo}) storage modified");
            if (!IsInitialized || IsLoadingStorage)
                return;

            Task.Run(() =>
            {
                dmc.CallMethod(async () => { await LoadStorage(); });
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
                while (IsInitializing && !IsInitialized)
                {
                    try
                    {
                        _log.Info($"Device ({SerialNo}), establishing remote device connection");
                        _remoteDevice = await _remoteDeviceFactory.CreateRemoteDeviceAsync(SerialNo, VERIFY_CHANNEL);
                        _remoteDevice.PropertyChanged += RemoteDevice_PropertyChanged;
                        _remoteDevice.StorageModified += RemoteDevice_StorageModified;

                        await _remoteDevice.Verify(VERIFY_CHANNEL);
                        await _remoteDevice.WaitVerification(VERIFY_WAIT);
                        await _remoteDevice.Initialize(INIT_WAIT);

                        if (_remoteDevice.SerialNo != SerialNo)
                        {
                            _remoteDevice.PropertyChanged -= RemoteDevice_PropertyChanged;
                            _remoteDevice.StorageModified -= RemoteDevice_StorageModified;
                            _serviceProxy.GetService().RemoveDevice(_remoteDevice.Id);
                            throw new Exception("Remote device serial number does not match enumerated serial number");
                        }

                        PasswordManager = new DevicePasswordManager(_remoteDevice, null);

                        FaultMessage = string.Empty;

                        _log.Info($"Remote device ({SerialNo}) initialized");
                    }
                    catch (FaultException<HideezServiceFault> ex)
                    {
                        _log.Error(ex.FormattedMessage());
                        FaultMessage = ex.FormattedMessage();
                    }
                    catch (HideezException ex)
                    {
                        _log.Error(ex);
                        FaultMessage = ex.Message;
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex);
                        FaultMessage = ex.Message;
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

        public async Task AuthorizeRemoteDevice()
        {
            if (!IsInitialized)
                throw new Exception("Remote not initialized"); // Todo: proper exception

            if (IsAuthorized || IsAuthorizing)
                return; // Remote is already authorized

            try
            {
                IsAuthorizing = true;

                if (_remoteDevice.AccessLevel.IsLocked)
                    throw new HideezException(HideezErrorCode.DeviceIsLocked);
                else if (_remoteDevice.AccessLevel.IsLinkRequired)
                    throw new HideezException(HideezErrorCode.DeviceRequiresLink);

                if (!await ButtonWorkflow())
                    throw new HideezException(HideezErrorCode.ButtonConfirmationTimeout);

                var pinTask = PinWorkflow();

                if (IsAuthorized)
                    _log.Info($"Remote device ({_remoteDevice.Id}) is authorized");
            }
            catch (FaultException<HideezServiceFault> ex)
            {
                _log.Error(ex.FormattedMessage());
            }
            catch (HideezException ex)
            {
                _log.Error(ex);
                FaultMessage = ex.Message;
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
            finally
            {
                _messenger.Send(new HidePinUiMessage());
                IsAuthorizing = false;
            }
        }

        async Task<bool> ButtonWorkflow()
        {
            if (!_remoteDevice.AccessLevel.IsButtonRequired)
                return true;

            _messenger.Send(new ShowButtonConfirmUiMessage(Id));
            var res = await _remoteDevice.WaitButtonConfirmation(CREDENTIAL_TIMEOUT);
            return res;
        }

        Task<bool> PinWorkflow()
        {
            if (_remoteDevice.AccessLevel.IsNewPinRequired)
            {
                return SetPinWorkflow();
            }
            else if (_remoteDevice.AccessLevel.IsPinRequired)
            {
                return EnterPinWorkflow();
            }

            return Task.FromResult(true);
        }

        async Task<bool> SetPinWorkflow()
        {
            var pin = await GetPin(Id, CREDENTIAL_TIMEOUT, withConfirm: true);
            if (pin == null || pin.Count() == 0)
                return false;
            bool res = await _remoteDevice.SetPin(Encoding.UTF8.GetString(pin)); //this using default timeout for BLE commands

            return res;
        }

        async Task<bool> EnterPinWorkflow()
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> PinWorkflow +++++++++++++++++++++++++++++++++++++++");

            bool pinOk = false;
            while (!_remoteDevice.AccessLevel.IsLocked)
            {
                var pin = await GetPin(Id, CREDENTIAL_TIMEOUT, withConfirm: true);
                Debug.WriteLine($">>>>>>>>>>>>>>> PIN: {pin}");
                if (pin == null || pin.Count() == 0)
                    break;

                pinOk = await _remoteDevice.EnterPin(Encoding.UTF8.GetString(pin)); //this using default timeout for BLE commands

                if (pinOk)
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> PIN OK");
                    break;
                }
                else
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> Wrong PIN ({_remoteDevice.PinAttemptsRemain} attempts left)");
                    if (_remoteDevice.AccessLevel.IsLocked)
                        _messenger.Send(new ShowErrorNotificationMessage($"Device is locked"));
                    else
                        _messenger.Send(new ShowErrorNotificationMessage($"Wrong PIN ({PinAttemptsRemain} attempts left)"));
                }
            }
            Debug.WriteLine(">>>>>>>>>>>>>>> PinWorkflow ------------------------------");
            return pinOk;
        }

        public async Task LoadStorage()
        {
            if (!IsInitialized)
                throw new Exception("Remote not initialized"); // Todo: proper exception

            if (!IsAuthorized)
                throw new HideezException(HideezErrorCode.ERR_UNAUTHORIZED);

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
            FaultMessage = string.Empty;
        }

        async Task<byte[]> GetPin(string deviceId, int timeout, bool withConfirm = false, bool askOldPin = false)
        {
            _messenger.Send(new ShowPinUiMessage(deviceId, withConfirm, askOldPin));

            var tcs = _pendingGetPinRequests.GetOrAdd(deviceId, (x) =>
            {
                return new TaskCompletionSource<byte[]>();
            });

            try
            {
                return await tcs.Task.TimeoutAfter(timeout);
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

        #region Old PIN

        public async Task<PinOperation> SetPinAsync(byte[] pin, CancellationToken cancellationToken)
        {
            PinOperation operationState = PinOperation.Unknown;
            // TODO: implement save PIN
            await Task.Delay(2_000);

            if (cancellationToken.IsCancellationRequested)
            {
                operationState = PinOperation.Canceled;
            }
            else
            {
                if (countAttemptsEnterPin >= 5)
                {
                    operationState = PinOperation.AccessDenied;
                }
                else
                {
                    operationState = Enumerable.SequenceEqual(pin, Encoding.UTF8.GetBytes("1234")) ? PinOperation.Successful : PinOperation.Error;
                }
            }

            return operationState;
        }

        public async Task<PinOperation> ChangePin(byte[] oldPin, byte[] newPin, CancellationToken cancellationToken)
        {
            PinOperation operationState = PinOperation.Unknown;
            // TODO: implement change PIN
            await Task.Delay(2_000);
            if (cancellationToken.IsCancellationRequested)
            {
                operationState = PinOperation.Canceled;
            }
            else
            {
                if (countAttemptsEnterPin >= 5)
                {
                    operationState = PinOperation.AccessDenied;
                }
                else
                {
                    operationState = Enumerable.SequenceEqual(oldPin, Encoding.UTF8.GetBytes("1234")) ? PinOperation.Successful : PinOperation.Error;
                }
            }

            return operationState;
        }

        public async Task<int> GetCountAttemptsEnterPinAsync()
        {
            // TODO: implement get count attempts enter pin
            await Task.Delay(1_000);

            return countAttemptsEnterPin;
        }

        int countAttemptsEnterPin = 0;
        public async Task<PinOperation> VerifyPinAsync(byte[] pin, CancellationToken cancellationToken)
        {
            PinOperation operationState = PinOperation.Unknown;
            // TODO: implement verify PIN
            await Task.Delay(2_000);

            if (cancellationToken.IsCancellationRequested)
            {
                operationState = PinOperation.Canceled;
            }
            else
            {
                if (countAttemptsEnterPin >= 5)
                {
                    operationState = PinOperation.AccessDenied;
                }
                else
                {
                    ++countAttemptsEnterPin;
                    if (Enumerable.SequenceEqual(pin, Encoding.UTF8.GetBytes("1234")))
                    {
                        operationState = PinOperation.Successful;
                        countAttemptsEnterPin = 0;
                    }
                    else
                    {
                        operationState = PinOperation.Error;
                    }
                }
            }

            return operationState;
        }

        #endregion PIN

    }
}
