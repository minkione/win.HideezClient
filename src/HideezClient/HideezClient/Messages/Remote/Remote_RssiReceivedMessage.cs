namespace HideezClient.Messages.Remote
{
    class Remote_RssiReceivedMessage : Remote_BaseMessage
    {
        public Remote_RssiReceivedMessage(string serialNo, double rssi)
            : base(serialNo)
        {
            Rssi = rssi;
        }


        public double Rssi { get; }
    }
}
