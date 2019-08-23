//using System;
//using System.Threading.Tasks;
//using Hideez.SDK.Communication.FW;
//using Hideez.SDK.Communication.Interfaces;
//using Hideez.SDK.Communication.Log;
//using Hideez.SDK.Communication.LongOperations;

//namespace DeviceMaintenance.ViewModel
//{
//    public class FirmwareUpdateViewModel : ViewModelBase
//    {
//        private readonly IDevice _device;
//        private readonly LongOperation _longOperation;
//        private readonly FirmwareImageUploader _imageUploader;

//        public string DeviceName => _device.SerialNo;
//        public Version CurrentFwVersion => _device.FirmwareVersion;
//        public double Progress => _longOperation.Progress;

//        public FirmwareUpdateViewModel(IDevice device, string fileName, EventLogger log)
//        {
//            _device = device;
//            _longOperation = new LongOperation(1);
//            _imageUploader = new FirmwareImageUploader(fileName, log);

//            _longOperation.StateChanged += LongOperation_StateChanged;
//        }

//        private void LongOperation_StateChanged(object sender, EventArgs e)
//        {
//            NotifyPropertyChanged(nameof(Progress));
//        }

//        public async Task RunAsync()
//        {
//            await _imageUploader.RunAsync(false, _device, _longOperation);
//        }
//    }
//}
