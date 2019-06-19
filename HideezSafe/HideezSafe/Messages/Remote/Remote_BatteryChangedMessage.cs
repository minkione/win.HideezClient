namespace HideezSafe.Messages.Remote
{
    class Remote_BatteryChangedMessage
    {
        public Remote_BatteryChangedMessage(string connectionId, int battery)
        {
            ConnectionId = connectionId;
            Battery = battery;
        }

        public string ConnectionId { get; }

        public int Battery { get; }
    }
}
