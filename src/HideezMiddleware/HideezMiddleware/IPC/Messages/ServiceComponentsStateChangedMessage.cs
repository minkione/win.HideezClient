using HideezMiddleware;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class ServiceComponentsStateChangedMessage : PubSubMessageBase
    {
        public HesStatus HesStatus { get; }
            
        public RfidStatus RfidStatus { get; }
            
        public BluetoothStatus DongleStatus { get; }
        
        public HesStatus TbHesStatus { get; }

        public BluetoothStatus BluetoothStatus { get; }

        public ServiceComponentsStateChangedMessage(HesStatus hesStatus, RfidStatus rfidStatus, BluetoothStatus dongleStatus, HesStatus tbHesStatus, BluetoothStatus bluetoothStatus)
        {
            HesStatus = hesStatus;
            RfidStatus = rfidStatus;
            DongleStatus = dongleStatus;
            TbHesStatus = tbHesStatus;
            BluetoothStatus = bluetoothStatus;
        }
    }
}
