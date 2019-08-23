namespace HideezSafe.Messages
{
    class ServiceComponentsStateChangedMessage
    {
        public bool HesConnected { get; set; }

        public bool RfidConnected { get; set; }

        public bool BleConnected { get; set; }

        public ServiceComponentsStateChangedMessage(bool hesConnected, bool rfidConnected, bool bleConnected)
        {
            HesConnected = hesConnected;
            RfidConnected = rfidConnected;
            BleConnected = bleConnected;
        }
    }
}
