namespace HideezSafe.Messages.Remote
{
    class Remote_RssiReceivedMessage
    {
        public Remote_RssiReceivedMessage(string connectionId, double rssi)
        {
            ConnectionId = connectionId;
            Rssi = rssi;
        }

        public string ConnectionId { get; }

        public double Rssi { get; }
    }
}
