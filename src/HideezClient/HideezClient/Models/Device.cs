using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
using HideezClient.HideezServiceReference;
using HideezClient.Modules;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using HideezClient.Utilities;
using MvvmExtensions.Attributes;
using NLog;
using System;
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

    public class Device : ObservableObject
    {
        readonly ILogger _log = LogManager.GetCurrentClassLogger();
        readonly IServiceProxy _serviceProxy;
        readonly IRemoteDeviceFactory _remoteDeviceFactory;
        RemoteDevice _remoteDevice;

        string id;
        string name;
        string ownerName;
        bool isConnected;
        string serialNo;
        double proximity;
        int battery;
        bool isInitializing;
        bool isInitialized;
        bool isAuthorizing;
        bool isAuthorized;
        bool isLoadingStorage;
        bool isStorageLoaded;
        private Version firmwareVersion;
        private Version bootloaderVersion;
        private uint storageTotalSize;
        private uint storageFreeSize;
        string faultMessage = string.Empty;
        private bool isVerifiedPin;

        public Device(IServiceProxy serviceProxy, IRemoteDeviceFactory remoteDeviceFactory)
        {
            _serviceProxy = serviceProxy;
            _remoteDeviceFactory = remoteDeviceFactory;

            RegisterDependencies();
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

        public string Id
        {
            get { return id; }
            set { Set(ref id, value); }
        }

        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                Set(ref isConnected, value);
                if (!isConnected)
                {
                    IsVerifiedPin = false;
                    Proximity = 0;
                    CloseRemoteDeviceConnection();
                }
            }
        }

        public double Proximity
        {
            get { return proximity; }
            set { Set(ref proximity, value); }
        }

        public int Battery
        {
            get { return battery; }
            set { Set(ref battery, value); }
        }

        public string OwnerName
        {
            get { return ownerName; }
            set { Set(ref ownerName, value); }
        }

        public string SerialNo
        {
            get { return serialNo; }
            set { Set(ref serialNo, value); }
        }

        public bool IsInitializing
        {
            get { return isInitializing; }
            private set { Set(ref isInitializing, value); }
        }

        public bool IsInitialized
        {
            get { return isInitialized; }
            private set { Set(ref isInitialized, value); }
        }

        public bool IsAuthorizing
        {
            get { return isAuthorizing; }
            private set { Set(ref isAuthorizing, value); }
        }

        public bool IsAuthorized
        {
            get { return IsAuthorized; }
            private set { Set(ref isAuthorized, value); }
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

        public Version FirmwareVersion
        {
            get { return firmwareVersion; }
            set { Set(ref firmwareVersion, value); }
        }

        public Version BootloaderVersion
        {
            get { return bootloaderVersion; }
            set { Set(ref bootloaderVersion, value); }
        }

        public uint StorageTotalSize
        {
            get { return storageTotalSize; }
            set { Set(ref storageTotalSize, value); }
        }

        public uint StorageFreeSize
        {
            get { return storageFreeSize; }
            set { Set(ref storageFreeSize, value); }
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
            set
            {
                Set(ref faultMessage, value);
            }
        }

        #region PIN

        public bool IsVerifiedPin
        {
            get { return isVerifiedPin; }
            protected set { Set(ref isVerifiedPin, value); }
        }

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
                        IsVerifiedPin = true;
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

        int remoteConnectionEstablishment = 0;
        public async Task EstablishRemoteDeviceConnection()
        {
            if (IsInitialized)
                return;

            if (Interlocked.CompareExchange(ref remoteConnectionEstablishment, 1, 0) == 0)
            {
                IsInitializing = true;

                // Allow other services to interact with the device first because
                // remote device connection establishmebt blocks communication
                // A 3s delay should be enough
                // Removing this line will result in slower workstation unlock
                await Task.Delay(3000);

                const int VERIFY_CHANNEL = 2;
                const int VERIFY_WAIT = 20_000;
                const int INIT_WAIT = 5_000;
                const int RETRY_DELAY = 2_500;

                try
                {
                    while (IsInitializing)
                    {
                        try
                        {
                            _log.Info($"Device ({SerialNo}), establishing remote device connection");
                            _remoteDevice = await _remoteDeviceFactory.CreateRemoteDeviceAsync(SerialNo, VERIFY_CHANNEL);

                            if (_remoteDevice == null)
                                continue;

                            await _remoteDevice.Verify(VERIFY_CHANNEL);
                            await _remoteDevice.WaitVerification(VERIFY_WAIT);
                            await _remoteDevice.Initialize(INIT_WAIT);

                            // Todo: Master password, Pin-code and Button flow

                            if (_remoteDevice.SerialNo != SerialNo)
                            {
                                _serviceProxy.GetService().RemoveDevice(_remoteDevice.DeviceId);
                                throw new Exception("Remote device serial number does not match enumerated serial number");
                            }

                            _remoteDevice.ProximityChanged += RemoteDevice_ProximityChanged;
                            _remoteDevice.BatteryChanged += RemoteDevice_BatteryChanged;
                            _remoteDevice.StorageModified += RemoteDevice_StorageModified;

                            Proximity = _remoteDevice.Proximity;
                            Battery = _remoteDevice.Battery;

                            _log.Info($"Device ({SerialNo}) connection established with remote device");

                            IsStorageLoaded = false;

                            IsLoadingStorage = true;

                            _log.Info($"Device ({SerialNo}) loading storage");

                            PasswordManager = new DevicePasswordManager(_remoteDevice, null);
                            await PasswordManager.Load();

                            _log.Info($"Device ({SerialNo}) loaded {PasswordManager.Accounts.Count} entries");

                            IsStorageLoaded = true;
                            IsInitialized = true;
                            FaultMessage = string.Empty;
                            break;
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

                            IsLoadingStorage = false;

                            if (IsInitializing)
                                await Task.Delay(RETRY_DELAY);
                        }
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref remoteConnectionEstablishment, 0);
                    IsInitializing = false;
                    IsLoadingStorage = false;
                }
            }
        }

        public void CloseRemoteDeviceConnection()
        {
            if (_remoteDevice != null)
            {
                _remoteDevice.ProximityChanged -= RemoteDevice_ProximityChanged;
                _remoteDevice.BatteryChanged -= RemoteDevice_BatteryChanged;
                _remoteDevice.StorageModified -= RemoteDevice_StorageModified;
                _remoteDevice = null;
                PasswordManager = null;

                Battery = 0;
                Proximity = 0;
            }

            IsInitialized = false;
            IsInitializing = false;
            IsStorageLoaded = false;
            IsLoadingStorage = false;
            FaultMessage = string.Empty;
        }

        void RemoteDevice_ProximityChanged(object sender, int proximity)
        {
            Proximity = proximity;
        }

        void RemoteDevice_BatteryChanged(object sender, int battery)
        {
            Battery = battery;
        }

        DelayedMethodCaller dmc = new DelayedMethodCaller(2000);

        void RemoteDevice_StorageModified(object sender, EventArgs e)
        {
            _log.Info($"Device ({SerialNo}) storage modified");
            if (!IsInitialized || IsLoadingStorage)
                return;

            Task.Run(() =>
            {
                dmc.CallMethod(async () => { await LoadStorageAsync(); });
            });

        }

        async Task LoadStorageAsync()
        {
            try
            {
                IsStorageLoaded = false;

                IsLoadingStorage = true;

                PasswordManager = new DevicePasswordManager(_remoteDevice, null);
                await PasswordManager.Load();
                _log.Info($"Device ({SerialNo}) reloaded {PasswordManager.Accounts.Count} entries");

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

        public void LoadFrom(DeviceDTO dto)
        {
            id = dto.Id;
            Name = dto.Name;
            OwnerName = dto.Owner ?? "...unspecified...";
            IsConnected = dto.IsConnected;
            SerialNo = dto.SerialNo;
            FirmwareVersion = dto.FirmwareVersion;
            BootloaderVersion = dto.BootloaderVersion;
            StorageTotalSize = dto.StorageTotalSize;
            StorageFreeSize = dto.StorageFreeSize;
        }
    }
}
