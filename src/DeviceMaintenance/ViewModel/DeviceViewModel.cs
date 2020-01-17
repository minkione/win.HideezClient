using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.FW;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.LongOperations;
using MvvmExtensions.Attributes;
using MvvmExtensions.PropertyChangedMonitoring;

namespace DeviceMaintenance.ViewModel
{
    public class DeviceViewModel : PropertyChangedImplementation
    {
        readonly string _mac;
        readonly LongOperation _longOperation = new LongOperation(1);

        bool startedUpdate = false;
        string customError = string.Empty;
        IDevice device = null;

        public delegate void FirmwareUpdateRequestEventHandler(IDevice device, LongOperation longOperation);
        public event FirmwareUpdateRequestEventHandler FirmwareUpdateRequest;

        public IDevice Device
        {
            get
            {
                return device;
            }
            private set
            {
                if (device != value)
                {
                    device = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Id => Device?.Id;
        public bool IsConnected => Device?.IsConnected ?? false;
        public int ChannelNo => Device?.ChannelNo ?? 0;
        public ConnectionState ConnectionState => Device?.IsConnected == true ? ConnectionState.Connected : ConnectionState.NotConnected;
        public string SerialNo => Device?.SerialNo ?? _mac;
        public Version FirmwareVersion => Device?.FirmwareVersion;
        public Version BootloaderVersion => Device?.BootloaderVersion;
        public uint StorageTotalSize => Device?.StorageTotalSize ?? 0;
        public uint StorageFreeSize => Device?.StorageFreeSize ?? 0;
        public bool IsInitialized => Device?.IsInitialized ?? false;
        public double Progress => _longOperation.Progress;
        public bool InProgress => _longOperation.IsRunning;
        [DependsOn(nameof(Error), nameof(CustomError))]
        public bool IsError
        {
            get
            {
                return (_longOperation != null && _longOperation.IsError) || !string.IsNullOrWhiteSpace(CustomError);
            }
        }
        public string Error => _longOperation.ErrorMessage;
        public string CustomError
        {
            get
            {
                return customError;
            }
            set
            {
                if (customError != value)
                {
                    customError = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool StartedUpdate
        { 
            get
            {
                return startedUpdate;
            }
            set
            {
                if (startedUpdate != value)
                {
                    startedUpdate = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #region Visual States
        [DependsOn(nameof(Device), nameof(IsConnected), nameof(IsInitialized), nameof(SuccessState), nameof(ErrorState))]
        public bool ConnectingState
        {
            get
            {
                return !IsConnected && !IsInitialized && !SuccessState && !ErrorState;
            }
        }

        [DependsOn(nameof(Device), nameof(IsConnected), nameof(IsInitialized), nameof(StartedUpdate))]
        public bool ReadyToUpdateState
        {
            get
            {
                return IsConnected && IsInitialized && !StartedUpdate;
            }
        }

        [DependsOn(nameof(Device), nameof(IsConnected), nameof(IsInitialized), nameof(StartedUpdate), nameof(InProgress), nameof(Progress))]
        public bool EnteringBootModeState
        {
            get
            {
                return IsConnected && IsInitialized && StartedUpdate && !InProgress && Progress == 0;
            }
        }
        [DependsOn(nameof(Device), nameof(IsConnected), nameof(IsInitialized), nameof(InProgress))]
        public bool UpdatingState
        {
            get
            {
                return IsConnected && IsInitialized && InProgress;
            }
        }
        [DependsOn(nameof(Device), nameof(InProgress), nameof(Progress), nameof(IsError))]
        public bool SuccessState
        {
            get
            {
                return !InProgress & Progress == 100 && !IsError;
            }
        }
        [DependsOn(nameof(Device), nameof(IsError))]
        public bool ErrorState
        {
            get
            {
                return IsError;
            }
        }
        #endregion

        public ICommand UpdateDevice
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        StartFirmwareUpdate();
                    }
                };
            }
        }

        public DeviceViewModel(string mac)
        {
            RegisterDependencies();

            _mac = mac;
            _longOperation.StateChanged += LongOperation_StateChanged;
        }

        void LongOperation_StateChanged(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(InProgress));
            NotifyPropertyChanged(nameof(Progress));
        }

        void Device_PropertyChanged(object sender, string e)
        {
            NotifyPropertyChanged(e);
        }

        public void SetDevice(IDevice device)
        {
            Device = device;

            Device.ConnectionStateChanged += (object sender, EventArgs e) =>
            {
                NotifyPropertyChanged(nameof(IsConnected));
                NotifyPropertyChanged(nameof(ConnectionState));
            };

            Device.PropertyChanged += Device_PropertyChanged;
        }

        public void StartFirmwareUpdate()
        {
            StartedUpdate = true;
            FirmwareUpdateRequest?.Invoke(Device, _longOperation);
        }
    }
}
