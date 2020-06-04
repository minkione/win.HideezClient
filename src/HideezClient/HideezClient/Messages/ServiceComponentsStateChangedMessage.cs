using HideezClient.HideezServiceReference;

namespace HideezClient.Messages
{
    class ServiceComponentsStateChangedMessage
    {
        public HesStatus HesStatus { get; set; }

        public RfidStatus RfidStatus { get; set; }

        public BluetoothStatus BluetoothStatus { get; set; }

        public HesStatus TbHesStatus { get; set; }

        public ServiceComponentsStateChangedMessage(HesStatus hesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus, HesStatus tbHesStatus)
        {
            HesStatus = hesStatus;
            RfidStatus = rfidStatus;
            BluetoothStatus = bluetoothStatus;
            TbHesStatus = tbHesStatus;
        }
    }
}
