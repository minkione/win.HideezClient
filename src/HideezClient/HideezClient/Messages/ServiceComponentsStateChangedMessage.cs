namespace HideezClient.Messages
{
    class ServiceComponentsStateChangedMessage
    {
        public bool HesConnected { get; set; }

        public bool ShowHesStatus { get; set; }

        public bool RfidConnected { get; set; }

        public bool ShowRfidStatus { get; set; }

        public bool BleConnected { get; set; }

        public bool TBHesConnected { get; set; }

        public ServiceComponentsStateChangedMessage(bool hesConnected, bool showHesStatus, bool rfidConnected, bool showRfidStatus, bool bleConnected, bool tbHesConnected)
        {
            HesConnected = hesConnected;
            ShowHesStatus = showHesStatus;
            RfidConnected = rfidConnected;
            ShowRfidStatus = showRfidStatus;
            BleConnected = bleConnected;
            TBHesConnected = tbHesConnected;
        }
    }
}
