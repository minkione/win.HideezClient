using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
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
using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;
using HideezMiddleware.IPC.IncommingMessages;
using HideezClient.Modules.Localize;

namespace HideezClient.Models
{
    // Todo: Implement thread-safety lock for password manager and remote device
    public class Device : ObservableObject, IDisposable
    {
        const int INIT_TIMEOUT = 5_000;
        readonly int CREDENTIAL_TIMEOUT = SdkConfig.MainWorkflowTimeout;

        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(Device));
        readonly IRemoteDeviceFactory _remoteDeviceFactory;
        readonly IMetaPubSub _metaMessenger;
        readonly IMetaPubSub _remoteDeviceMessenger = new MetaPubSub(new MetaPubSubLogger(new NLogWrapper()));

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
        string mac;
        uint storageTotalSize;
        uint storageFreeSize;
        Version firmwareVersion;
        Version bootloaderVersion;
        int pinAttemptsRemain;
        bool isCanUnlock;
        int minPinLength;
        int unlockAttemptsRemain;
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
        CancellationTokenSource remoteCancellationTokenSource;

        bool _isStorageLocked = false;

        public Device(
            IRemoteDeviceFactory remoteDeviceFactory,
            IMetaPubSub metaMessenger,
            DeviceDTO dto)
        {
            _remoteDeviceFactory = remoteDeviceFactory;
            _metaMessenger = metaMessenger;

            PropertyChanged += Device_PropertyChanged;

            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceConnectionStateChangedMessage>(OnDeviceConnectionStateChanged);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceInitializedMessage>(OnDeviceInitialized);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceFinishedMainFlowMessage>(OnDeviceFinishedMainFlow);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceOperationCancelledMessage>(OnOperationCancelled);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceProximityChangedMessage>(OnDeviceProximityChanged);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceBatteryChangedMessage>(OnDeviceBatteryChanged);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceProximityLockEnabledMessage>(OnDeviceProximityLockEnabled);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.LockDeviceStorageMessage>(OnLockDeviceStorage);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.LiftDeviceStorageLockMessage>(OnLiftDeviceStorageLock);
            _metaMessenger.Subscribe<SendPinMessage>(OnPinReceived);
            _metaMessenger.Subscribe<SessionSwitchMessage>(OnSessionSwitch);

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

        public string TypeName { get; } = "Hardware Vault";

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

        //Todo: Add this property in contract on service
        public string Mac
        {
            get { return mac; }
            private set { Set(ref mac, value); }
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

        public int OtherConnections
        {
            get { return _remoteDevice != null ? (int)_remoteDevice?.OtherConnections : 0; }
        }

        public bool IsCanUnlock
        {
            get { return isCanUnlock; }
            private set { Set(ref isCanUnlock, value); }
        }

        public int MinPinLength
        {
            get { return minPinLength; }
            private set { Set(ref minPinLength, value); }
        }

        public int UnlockAttemptsRemain
        {
            get { return unlockAttemptsRemain; }
            private set { Set(ref unlockAttemptsRemain, value); }
        }

        public bool IsStorageLocked
        {
            get { return _isStorageLocked; }
            private set { Set(ref _isStorageLocked, value); }
        }
        #endregion

        #region Messege & Event handlers
        void RemoteDevice_ButtonPressed(object sender, ButtonPressCode e)
        {
            _log.WriteLine($"({SerialNo}) Vault button pressed, code: {e}");

            Task.Run(() =>
            {
                _metaMessenger.Publish(new DeviceButtonPressedMessage(Id, e));
            });
        }

        void RemoteDevice_StorageModified(object sender, EventArgs e)
        {
            _log.WriteLine($"({SerialNo}) Vault storage modified");

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

        async Task OnDeviceConnectionStateChanged(HideezMiddleware.IPC.Messages.DeviceConnectionStateChangedMessage obj)
        {
            if (obj.Device.Id == Id)
            {
                LoadFrom(obj.Device);

                if (!obj.Device.IsConnected)
                    await ShutdownRemoteDeviceAsync(HideezErrorCode.DeviceDisconnected);
            }
        }

        Task OnDeviceInitialized(HideezMiddleware.IPC.Messages.DeviceInitializedMessage obj)
        {
            if (obj.Device.Id == Id)
                LoadFrom(obj.Device);
            
            return Task.CompletedTask;
        }

        Task OnDeviceFinishedMainFlow(HideezMiddleware.IPC.Messages.DeviceFinishedMainFlowMessage obj)
        {
            if (obj.Device.Id == Id)
                LoadFrom(obj.Device);

            return Task.CompletedTask;
        }

        Task OnPinReceived(SendPinMessage obj)
        {
            if (_pendingGetPinRequests.TryGetValue(obj.DeviceId, out TaskCompletionSource<byte[]> tcs))
                tcs.TrySetResult(obj.Pin);

            return Task.CompletedTask;
        }

        Task OnSessionSwitch(SessionSwitchMessage obj)
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
                    return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        Task OnOperationCancelled(HideezMiddleware.IPC.Messages.DeviceOperationCancelledMessage obj)
        {
            if (obj.Device.Id == Id)
            {
                CancelDeviceAuthorization();
            }

            return Task.CompletedTask;
        }

        Task OnDeviceProximityChanged(HideezMiddleware.IPC.Messages.DeviceProximityChangedMessage obj)
        {
            // Todo: MAYBE it will be beneficial to add a check that device is connected
            if (Id != obj.DeviceId)
                return Task.CompletedTask;

            Proximity = obj.Proximity;

            return Task.CompletedTask;
        }

        Task OnDeviceBatteryChanged(HideezMiddleware.IPC.Messages.DeviceBatteryChangedMessage obj)
        {
            if (Id != obj.DeviceId)
                return Task.CompletedTask;

            Battery = obj.Battery;

            return Task.CompletedTask;
        }

        Task OnDeviceProximityLockEnabled(HideezMiddleware.IPC.Messages.DeviceProximityLockEnabledMessage obj)
        {
            if (obj.Device.Id == Id)
                LoadFrom(obj.Device);

            return Task.CompletedTask;
        }

        Task OnLockDeviceStorage(HideezMiddleware.IPC.Messages.LockDeviceStorageMessage obj)
        {
            _log.WriteLine($"Lock vault storage ({obj.SerialNo})");

            if (obj.SerialNo == SerialNo)
            {
                IsStorageLocked = true;
                _metaMessenger.Publish(new ShowInfoNotificationMessage(string.Format(TranslationSource.Instance["Vault.Notification.Synchronizing"], serialNo, Environment.NewLine), notificationId: Mac));
            }

            return Task.CompletedTask;
        }

        Task OnLiftDeviceStorageLock(HideezMiddleware.IPC.Messages.LiftDeviceStorageLockMessage obj)
        {
            _log.WriteLine($"Lift vault storage lock ({obj.SerialNo})");

            if (obj.SerialNo == SerialNo || string.IsNullOrWhiteSpace(obj.SerialNo))
                IsStorageLocked = false;

            return Task.CompletedTask;
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
            IsCanUnlock = dto.IsCanUnlock;
            MinPinLength = dto.MinPinLength;
            UnlockAttemptsRemain = dto.UnlockAttemptsRemain;
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

        void CancelRemoteDeviceCreation()
        {
            remoteCancellationTokenSource?.Cancel();
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

                            remoteCancellationTokenSource = new CancellationTokenSource();

                            _infNid = Guid.NewGuid().ToString();
                            _errNid = Guid.NewGuid().ToString();

                            await CreateRemoteDeviceAsync(remoteCancellationTokenSource.Token);

                            if (remoteCancellationTokenSource.IsCancellationRequested)
                            {
                                _log.WriteLine($"({SerialNo}) Remote vault creation cancelled");
                                return;
                            }

                            if (_remoteDevice != null)
                            {
                                _log.WriteLine($"({_remoteDevice.SerialNo}) Remote vault created");
                                await _remoteDevice.RefreshDeviceInfo();
                                if (_remoteDevice.AccessLevel != null)
                                {
                                    _log.WriteLine($"({_remoteDevice.SerialNo}) access profile (allOk:{_remoteDevice.AccessLevel.IsAllOk}; " +
                                        $"pin:{_remoteDevice.AccessLevel.IsPinRequired}; " +
                                        $"newPin:{_remoteDevice.AccessLevel.IsNewPinRequired}; " +
                                        $"button:{_remoteDevice.AccessLevel.IsButtonRequired}; " +
                                        $"link:{_remoteDevice.AccessLevel.IsLinkRequired}; " +
                                        $"master:{_remoteDevice.AccessLevel.IsMasterKeyRequired}; " +
                                        $"locked:{_remoteDevice.AccessLevel.IsLocked})");
                                }
                                else
                                    _log.WriteLine($"({_remoteDevice.SerialNo}) access level is null");
                            }

                            if (_remoteDevice.IsLockedByCode)
                                throw new HideezException(HideezErrorCode.DeviceIsLocked);
                            else if (authorizeDevice && !IsAuthorized && _remoteDevice?.AccessLevel != null && !_remoteDevice.AccessLevel.IsAllOk)
                                await AuthorizeRemoteDevice(ct);
                            else if (!authorizeDevice && !IsAuthorized && _remoteDevice?.AccessLevel != null && _remoteDevice.AccessLevel.IsAllOk)
                                await AuthorizeRemoteDevice(ct);
                            else if (_remoteDevice?.AccessLevel != null && _remoteDevice.AccessLevel.IsLocked)
                                throw new HideezException(HideezErrorCode.DeviceIsLocked);

                            if (!ct.IsCancellationRequested && !IsStorageLoaded)
                                await LoadStorage();
                        }
                        catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.DeviceDisconnected)
                        {
                            _log.WriteLine("Remote vault creation aborted, vault disconnected", LogErrorSeverity.Warning);
                        }
                        catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.DeviceIsLocked)
                        {
                            if (_remoteDevice.IsLockedByCode)
                            {
                                await _metaMessenger.Publish(new ShowLockNotificationMessage(TranslationSource.Instance["Notification.DeviceLockedByCode.Message"],
                                    TranslationSource.Instance["Notification.DeviceLockedByCode.Caption"],
                                    new NotificationOptions() { CloseTimeout = NotificationOptions.LongTimeout },
                                    Mac));
                            }
                            else if (_remoteDevice.IsLockedByPin)
                            {
                                await _metaMessenger.Publish(new ShowLockNotificationMessage(TranslationSource.Instance["Notification.DeviceLockedByPin.Message"],
                                    TranslationSource.Instance["Notification.DeviceLockedByPin.Caption"],
                                    new NotificationOptions() { CloseTimeout = NotificationOptions.LongTimeout },
                                    Mac));
                            }
                            else
                            {
                                _log.WriteLine(ex);
                            }
                        }
                        catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.DeviceIsLockedByPin)
                        {
                        }
                        catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.DeviceIsLockedByCode)
                        {
                        }
                        catch (Exception ex)
                        {
                            ShowError(ex.Message, _errNid);
                        }
                        finally
                        {
                            var tmp = authCancellationTokenSource;
                            authCancellationTokenSource = null;
                            tmp.Dispose();

                            tmp = remoteCancellationTokenSource;
                            remoteCancellationTokenSource = null;
                            tmp.Dispose();
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
                    CancelRemoteDeviceCreation();
                    CancelDeviceAuthorization();

                    var tempRemoteDevice = _remoteDevice;
                    _remoteDevice = null;
                    PasswordManager = null;

                    if (tempRemoteDevice != null)
                    {
                        tempRemoteDevice.ButtonPressed -= RemoteDevice_ButtonPressed;
                        tempRemoteDevice.StorageModified -= RemoteDevice_StorageModified;
                        tempRemoteDevice.PropertyChanged -= RemoteDevice_PropertyChanged;
                        await tempRemoteDevice.Shutdown(code);
                        await _metaMessenger.PublishOnServer(new RemoveDeviceMessage(tempRemoteDevice.Id));
                        await tempRemoteDevice.DeleteContext();
                        tempRemoteDevice.Dispose();
                    }
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

        async Task CreateRemoteDeviceAsync(CancellationToken cancellationToken)
        {
            if (_remoteDevice != null || IsCreatingRemoteDevice)
                return;

            HideezErrorCode initErrorCode = HideezErrorCode.Ok;

            try
            {
                _log.WriteLine($"({SerialNo}) Establishing remote vault connection");

                if (cancellationToken.IsCancellationRequested)
                {
                    initErrorCode = HideezErrorCode.RemoteDeviceCreationCancelled;
                    return;
                }

                _log.WriteLine("Checking for available channels");
                var channelsReply = await _metaMessenger.ProcessOnServer<GetAvailableChannelsMessageReply>(new GetAvailableChannelsMessage(SerialNo), 0);
                var channels = channelsReply.FreeChannels;
                if (channels.Length == 0)
                    throw new Exception(string.Format(TranslationSource.Instance["Vault.Error.NoAvailableChannels"], SerialNo)); // Todo: separate exception type
                var channelNo = channels.FirstOrDefault();
                _log.WriteLine($"{channels.Length} channels available");

                if (cancellationToken.IsCancellationRequested)
                {
                    initErrorCode = HideezErrorCode.RemoteDeviceCreationCancelled;
                    return;
                }

                ShowInfo(string.Format(TranslationSource.Instance["Vault.Notification.PreparingForAuth"], SerialNo), Mac);
                IsCreatingRemoteDevice = true;
                _remoteDevice = await _remoteDeviceFactory.CreateRemoteDeviceAsync(SerialNo, channelNo);
                await _remoteDeviceMessenger.TryConnectToServer(_remoteDevice.RemoteDeviceConnectionId);
                _remoteDevice.PropertyChanged += RemoteDevice_PropertyChanged;

                if (cancellationToken.IsCancellationRequested)
                {
                    initErrorCode = HideezErrorCode.RemoteDeviceCreationCancelled;
                    return;
                }

                await _remoteDevice.VerifyAndInitialize(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    initErrorCode = HideezErrorCode.RemoteDeviceCreationCancelled;
                    return;
                }

                if (_remoteDevice.SerialNo != SerialNo)
                    throw new Exception(TranslationSource.Instance["Vault.Error.InvalidRemoteSerialNo"]);

                _log.WriteLine($"({SerialNo}) Creating password manager");
                PasswordManager = new DevicePasswordManager(_remoteDevice, null);
                _remoteDevice.StorageModified += RemoteDevice_StorageModified;
                _remoteDevice.ButtonPressed += RemoteDevice_ButtonPressed;

                _log.WriteLine($"({SerialNo}) Remote vault connection established");
            }
            catch (HideezException ex)
            {
                _log.WriteLine(ex);
                ShowError(ex.Message, Mac);
                initErrorCode = ex.ErrorCode;
                await _remoteDeviceMessenger.DisconnectFromServer();
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
                ShowError(ex.Message, Mac);
                initErrorCode = HideezErrorCode.UnknownError;
                await _remoteDeviceMessenger.DisconnectFromServer();
            }
            finally
            {
                await _metaMessenger.Publish(new HidePinUiMessage());

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
                    ShowError(string.Format(TranslationSource.Instance["Vault.Error.AuthCanceled"], SerialNo), Mac);
                else if (IsAuthorized)
                    _log.WriteLine($"({_remoteDevice.Id}) Remote vault authorized");
                else
                    ShowError(string.Format(TranslationSource.Instance["Vault.Error.AuthFailed"], SerialNo), Mac);
            }
            catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.DeviceIsLocked || ex.ErrorCode == HideezErrorCode.DeviceIsLockedByPin)
            {
                _log.WriteLine($"({Mac}) Auth failed. Vault is locked due to too many incorrect PIN entries");
                await _metaMessenger.Publish(new ShowLockNotificationMessage(TranslationSource.Instance["Notification.DeviceLockedByPin.Message"],
                                    TranslationSource.Instance["Notification.DeviceLockedByPin.Caption"],
                                    new NotificationOptions() { CloseTimeout = NotificationOptions.LongTimeout },
                                    Mac));
            }
            catch (HideezException ex)
            {
                _log.WriteLine(ex);
                ShowError(ex.Message, Mac);
            }
            catch (OperationCanceledException ex)
            {
                _log.WriteLine(ex);
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
                ShowError(ex.Message, Mac);
            }
            finally
            {
                await _metaMessenger.Publish(new HidePinUiMessage());

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

                _log.WriteLine($"({serialNo}) Loading storage");
                await PasswordManager.Load();
                _log.WriteLine($"({serialNo}) Loaded {PasswordManager.Accounts.Count()} entries from storage");
                
                IsStorageLoaded = true;
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
                ShowError(ex.Message, Mac);

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

            ShowInfo(TranslationSource.Instance["Vault.Notification.PressButton"], Mac);
            await _metaMessenger.Publish(new ShowButtonConfirmUiMessage(Id));
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
                ShowInfo(TranslationSource.Instance["Vault.Notification.NewPin"], Mac);
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
                    ShowError(TranslationSource.Instance["Vault.Error.PinToShort"], Mac);
                }
                else if (pinResult == HideezErrorCode.ERR_PIN_WRONG)
                {
                    ShowError(TranslationSource.Instance["Vault.Error.InvalidPin"], Mac);
                }
            }
            Debug.WriteLine(">>>>>>>>>>>>>>> SetPinWorkflow ---------------------------------------");
            return pinOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Returns true is pin workflow successful. Returns false if workflow cancelled.</returns>
        /// <exception cref="HideezException">Thrown with code <see cref="HideezErrorCode.DeviceIsLocked"/> if device is locked due to failed attempt</exception>
        async Task<bool> EnterPinWorkflow(CancellationToken ct)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> EnterPinWorkflow +++++++++++++++++++++++++++++++++++++++");

            bool pinOk = false;
            while (!AccessLevel.IsLocked)
            {
                ShowInfo(TranslationSource.Instance["Vault.Notification.EnterCurrentPin"], Mac);
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

                var attemptsLeft = PinAttemptsRemain - 1;
                var pinResult = await _remoteDevice.EnterPin(Encoding.UTF8.GetString(pin)); //this using default timeout for BLE commands

                if (pinResult == HideezErrorCode.Ok)
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> PIN OK");
                    pinOk = true;
                    break;
                }
                else if (pinResult == HideezErrorCode.ERR_DEVICE_LOCKED_BY_PIN)
                {
                    throw new HideezException(HideezErrorCode.DeviceIsLocked);
                }
                else // ERR_PIN_WRONG and ERR_PIN_TOO_SHORT should just be displayed as wrong pin for security reasons
                {
                    Debug.WriteLine($">>>>>>>>>>>>>>> Wrong PIN ({attemptsLeft} attempts left)");
                    if (AccessLevel.IsLocked)
                        ShowError(TranslationSource.Instance["Vault.Error.VaultLocked"], Mac);
                    else
                    {
                        ShowError(string.Format(TranslationSource.Instance["Vault.Error.WrongPin"], attemptsLeft), Mac);
                        await _remoteDevice.RefreshDeviceInfo(); // Remaining pin attempts update is not quick enough 
                    }
                }
            }
            Debug.WriteLine(">>>>>>>>>>>>>>> PinWorkflow ------------------------------");
            return pinOk;
        }

        async Task<byte[]> GetPin(string deviceId, int timeout, CancellationToken ct, bool withConfirm = false, bool askOldPin = false)
        {
            await _metaMessenger.Publish(new ShowPinUiMessage(deviceId, withConfirm, askOldPin));

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
            _metaMessenger?.Publish(new ShowInfoNotificationMessage(message, notificationId: notificationId));
        }

        void ShowError(string message, string notificationId)
        {
            _metaMessenger?.Publish(new ShowErrorNotificationMessage(message, notificationId: notificationId));
        }

        void ShowWarn(string message, string notificationId)
        {
            _metaMessenger?.Publish(new ShowWarningNotificationMessage(message, notificationId: notificationId));
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
                    _metaMessenger.Unsubscribe<HideezMiddleware.IPC.Messages.DeviceConnectionStateChangedMessage>(OnDeviceConnectionStateChanged);
                    _metaMessenger.Unsubscribe<HideezMiddleware.IPC.Messages.DeviceInitializedMessage>(OnDeviceInitialized);
                    _metaMessenger.Unsubscribe<HideezMiddleware.IPC.Messages.DeviceFinishedMainFlowMessage>(OnDeviceFinishedMainFlow);
                    _metaMessenger.Unsubscribe<HideezMiddleware.IPC.Messages.DeviceOperationCancelledMessage>(OnOperationCancelled);
                    _metaMessenger.Unsubscribe<HideezMiddleware.IPC.Messages.DeviceProximityChangedMessage>(OnDeviceProximityChanged);
                    _metaMessenger.Unsubscribe<HideezMiddleware.IPC.Messages.DeviceBatteryChangedMessage>(OnDeviceBatteryChanged);
                    _metaMessenger.Unsubscribe<HideezMiddleware.IPC.Messages.DeviceProximityLockEnabledMessage>(OnDeviceProximityLockEnabled);
                    _metaMessenger.Unsubscribe<HideezMiddleware.IPC.Messages.LockDeviceStorageMessage>(OnLockDeviceStorage);
                    _metaMessenger.Unsubscribe<HideezMiddleware.IPC.Messages.LiftDeviceStorageLockMessage>(OnLiftDeviceStorageLock);
                    _metaMessenger.Unsubscribe<SendPinMessage>(OnPinReceived);
                    _metaMessenger.Unsubscribe<SessionSwitchMessage>(OnSessionSwitch);
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
