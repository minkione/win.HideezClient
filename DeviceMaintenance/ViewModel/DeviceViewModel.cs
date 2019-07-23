using System;
using System.Threading.Tasks;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.FW;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.LongOperations;

namespace DeviceMaintenance.ViewModel
{
    public class DeviceViewModel : ViewModelBase
    {
        private readonly ILog _log;
        readonly string _mac;
        readonly LongOperation _longOperation = new LongOperation(1);
        FirmwareImageUploader _imageUploader;

        public IDevice Device { get; private set; }

        public string Id => Device?.Id;
        public bool IsConnected => Device?.IsConnected ?? false;
        public int ChannelNo => Device?.ChannelNo ?? 0;
        public ConnectionState ConnectionState => Device?.Connection.State ?? ConnectionState.NotConnected;

        public string SerialNo => Device?.SerialNo ?? _mac;
        public Version FirmwareVersion => Device?.FirmwareVersion;
        public Version BootloaderVersion => Device?.BootloaderVersion;
        public uint StorageTotalSize => Device?.StorageTotalSize ?? 0;
        public uint StorageFreeSize => Device?.StorageFreeSize ?? 0;
        public bool IsInitialized => Device?.IsInitialized ?? false;
        public double Progress => _longOperation.Progress;

        public bool InProgress => _longOperation.IsRunning;



        public DeviceViewModel(string mac, ILog log)
        {
            _log = log;
            _mac = mac;
            _longOperation.StateChanged += LongOperation_StateChanged;
        }

        public void SetDevice(IDevice device)
        {
            Device = device;

            Device.ConnectionStateChanged += (object sender, EventArgs e)
                => NotifyPropertyChanged(nameof(IsConnected));

            Device.PropertyChanged += Device_PropertyChanged;
        }

        void LongOperation_StateChanged(object sender, EventArgs e)
        {
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
