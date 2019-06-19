namespace HideezSafe.Messages.Remote
{
    class Remote_RssiReceivedMessage
    {
        public Remote_RssiReceivedMessage(string serialNo, double rssi)
        {
            SerialNo = serialNo;
            Rssi = rssi;
        }

        public string SerialNo { get; }

        public double Rssi { get; }
    }
}
