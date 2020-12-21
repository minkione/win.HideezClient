using DeviceMaintenance.ViewModel;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.LongOperations;
using Meta.Lib.Modules.PubSub;

namespace DeviceMaintenance.Messages
{
    public class EnterBootCommand : PubSubMessageBase
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
