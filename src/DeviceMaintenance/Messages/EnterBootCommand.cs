using DeviceMaintenance.ViewModel;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.LongOperations;

namespace DeviceMaintenance.Messages
{
    public class EnterBootCommand : MessageBase
    {
        public DeviceViewModel DeviceViewModel { get; }
        public IDevice Device { get; }
        public LongOperation LongOperation { get; }
        public string FirmwareFilePath { get; }

        public EnterBootCommand(DeviceViewModel deviceViewModel,
            string firmwareFilePath, IDevice device, LongOperation longOperation)
        {
            DeviceViewModel = deviceViewModel;
            FirmwareFilePath = firmwareFilePath;
            Device = device;
            LongOperation = longOperation;
        }
    }
}
