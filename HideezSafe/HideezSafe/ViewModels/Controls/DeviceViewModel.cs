using Hideez.SDK.Communication.Remote;
using HideezSafe.HideezServiceReference;
using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Mvvm;
using MvvmExtensions.Commands;
using NLog;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HideezSafe.ViewModels
{
    public class DeviceViewModel : LocalizedObject
    {
        readonly ILogger _log = LogManager.GetCurrentClassLogger();
        readonly IWindowsManager _windowsManager;
        readonly IServiceProxy _serviceProxy;
        readonly IRemoteDeviceFactory _remoteDeviceFactory;

        string id;
        string name;
        string ownerName;
        bool isConnected;
        string serialNo;
        double proximity;
        int battery;

        string typeNameKey = "Hideez key";

        RemoteDevice RemoteDevice;

        public DeviceViewModel(DeviceDTO device, IWindowsManager windowsManager, 
            IServiceProxy serviceProxy, IRemoteDeviceFactory remoteDeviceFactory)
        {
            _windowsManager = windowsManager;
            _serviceProxy = serviceProxy;
            _remoteDeviceFactory = remoteDeviceFactory;
            LoadFrom(device);
        }

        #region Properties
        
        public string IcoKey { get; } = "HedeezKeySimpleIMG";

        public string Id
        {
            get { return id; }
            set { Set(ref id, value); }
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

        [Localization]
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        [Localization]
        public string TypeName
        {
            get { return L(typeNameKey); }
            set { Set(ref typeNameKey, value); }
        }

        #endregion Property

        #region Commands

        public ICommand AddCredentialCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        _windowsManager.ShowDialogAddCredential(this);
                    },
                };
            }
        }

        public ICommand DisconnectDeviceCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnDisconnectDevice();
                    },
                };
            }
        }

        public ICommand RemoveDeviceCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnRemoveDevice();
                    },
                };
            }
        }

        #endregion

        public void LoadFrom(DeviceDTO dto)
        {
            id = dto.Id;
            Name = dto.Name;
            OwnerName = dto.Owner ?? "...unspecified...";
            IsConnected = dto.IsConnected;
            SerialNo = dto.SerialNo;
        }

        async void OnDisconnectDevice()
        {
            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to disconnect {Name}?", 
                    $"Disconnect {Name}", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    await _serviceProxy.GetService().DisconnectDeviceAsync(Id);
            }
            catch (Exception ex)
            {
                _windowsManager.ShowError(ex.Message);
            }
        }

        async void OnRemoveDevice()
        {
            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove {Name}?{Environment.NewLine}Note: All manually stored data will be lost!", 
                    $"Remove {Name}", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    await _serviceProxy.GetService().RemoveDeviceAsync(Id);
            }
            catch (Exception ex)
            {
                _windowsManager.ShowError(ex.Message);
            }
        }


        public async Task EstablishRemoteDeviceConnection()
        {
            CloseRemoteDeviceConnection();

            RemoteDevice = await _remoteDeviceFactory.CreateRemoteDeviceAsync(SerialNo, 2);
            await RemoteDevice.Authenticate(2);
            await RemoteDevice.WaitAuthentication(20_000);
            await RemoteDevice.Initialize(10_000);

            if (RemoteDevice.SerialNo != SerialNo)
            {
                _serviceProxy.GetService().RemoveDevice(RemoteDevice.DeviceId);
                throw new Exception("Remote device serial number does not match enumerated serial number");
            }

            RemoteDevice.ProximityChanged += RemoteDevice_ProximityChanged;
            RemoteDevice.BatteryChanged += RemoteDevice_BatteryChanged;

            Proximity = RemoteDevice.Proximity;
            Battery = RemoteDevice.Battery;
        }

        public void CloseRemoteDeviceConnection()
        {
            if (RemoteDevice != null)
            {
                RemoteDevice.ProximityChanged -= RemoteDevice_ProximityChanged;
                RemoteDevice.BatteryChanged -= RemoteDevice_BatteryChanged;

                RemoteDevice = null;

                NotifyPropertyChanged(nameof(Proximity));
                NotifyPropertyChanged(nameof(Battery));
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
    }
}
