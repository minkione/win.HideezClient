using HideezMiddleware;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class ServiceComponentsStateChangedMessage : PubSubMessageBase
    {
        public HesStatus HesStatus { get; }
            
        public RfidStatus RfidStatus { get; }
            
        public BluetoothStatus BluetoothStatus { get; }
        
        public HesStatus TbHesStatus { get; }

        public ServiceComponentsStateChangedMessage(HesStatus hesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus, HesStatus tbHesStatus)
        {
            HesStatus = hesStatus;
            RfidStatus = rfidStatus;
            BluetoothStatus = bluetoothStatus;
            TbHesStatus = tbHesStatus;
        }
    }
}
