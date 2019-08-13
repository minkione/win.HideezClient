using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
using HideezSafe.HideezServiceReference;
using HideezSafe.Modules;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Mvvm;
using HideezSafe.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HideezSafe.Models
{
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
        bool isLoadingStorage;
        bool isStorageLoaded;
        private Version firmwareVersion;
        private Version bootloaderVersion;
        private uint storageTotalSize;
        private uint storageFreeSize;

        public Device(IServiceProxy serviceProxy, IRemoteDeviceFactory remoteDeviceFactory)
        {
            _serviceProxy = serviceProxy;
            _remoteDeviceFactory = remoteDeviceFactory;
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

                const int AUTH_CHANNEL = 2;
                const int AUTH_WAIT = 20_000;
                const int INIT_WAIT = 5_000;
                const int RETRY_DELAY = 2_500;

                try
                {
                    while (IsInitializing)
                    {
                        try
                        {
                            _log.Info($"Device ({SerialNo}), establishing remote device connection");
                            _remoteDevice = await _remoteDeviceFactory.CreateRemoteDeviceAsync(SerialNo, AUTH_CHANNEL);

                            if (_remoteDevice == null)
                                continue;

                            await _remoteDevice.Authenticate(AUTH_CHANNEL, null);
                            await _remoteDevice.WaitAuthentication(AUTH_WAIT);
                            await _remoteDevice.Initialize(INIT_WAIT);

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

                            _log.Info($"Device ({SerialNo}) loading storage");

                            PasswordManager = new DevicePasswordManager(_remoteDevice, null);
                            await PasswordManager.Load();

                            _log.Info($"Device ({SerialNo}) loaded {PasswordManager.Accounts.Count} entries");

                            IsStorageLoaded = true;
                            IsInitialized = true;
                            break;
                        }
                        catch (FaultException<HideezServiceFault> ex)
                        {
                            _log.Error(ex.FormattedMessage());
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex);
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

                IsInitialized = false;
                isInitializing = false;
                IsStorageLoaded = false;
                IsLoadingStorage = false;
            }
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
