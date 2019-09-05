namespace HideezClient.Messages.Remote
{
    class Remote_RssiReceivedMessage : Remote_BaseMessage
    {
        public Remote_RssiReceivedMessage(string serialNo, sbyte rssi)
            : base(serialNo)
        {
            Rssi = rssi;
        }


        public sbyte Rssi { get; }
    }
}
