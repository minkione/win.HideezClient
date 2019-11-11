using System;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
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
        private readonly ILog _log;
        readonly string _mac;
        readonly LongOperation _longOperation = new LongOperation(1);
        FirmwareImageUploader _imageUploader;

        public IDevice Device { get; private set; }

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
        public string CustomError { get; set; }


        #region Visual States
        [DependsOn(nameof(IsConnected), nameof(IsInitialized))]
        public bool ConnectingState
        {
            get
            {
                return IsConnected && !IsInitialized;
            }
        }
        [DependsOn(nameof(IsConnected), nameof(IsInitialized), nameof(InProgress), nameof(Progress))]
        public bool StartingUpdateState
        {
            get
            {
                return IsConnected && IsInitialized && !InProgress && Progress == 0;
            }
        }
        [DependsOn(nameof(IsConnected), nameof(IsInitialized), nameof(InProgress))]
        public bool UpdatingState
        {
            get
            {
                return IsConnected && IsInitialized && InProgress;
            }
        }
        [DependsOn(nameof(InProgress), nameof(Progress), nameof(IsError))]
        public bool SuccessState
        {
            get
            {
                return !InProgress & Progress == 100 && !IsError;
            }
        }
        [DependsOn(nameof(IsError))]
        public bool ErrorState
        {
            get
            {
                return IsError;
            }
        }
        #endregion

        public DeviceViewModel(string mac, ILog log)
        {
            _log = log;
            _mac = mac;
            _longOperation.StateChanged += LongOperation_StateChanged;
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

        void LongOperation_StateChanged(object sender, EventArgs e)
        {
            NotifyPropertyChanged(nameof(InProgress));
            NotifyPropertyChanged(nameof(Progress));
        }

        void Device_PropertyChanged(object sender, string e)
        {
            NotifyPropertyChanged(e);
        }

        public async Task StartFirmwareUpdate(string fileName)
        {
            _imageUploader = new FirmwareImageUploader(fileName, _log);
            await _imageUploader.RunAsync(false, Device, _longOperation);
        }
    }
}
