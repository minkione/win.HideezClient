namespace HideezClient.Messages.Remote
{
    class Remote_BatteryChangedMessage : Remote_BaseMessage
    {
        public Remote_BatteryChangedMessage(string serialNo, int battery)
            : base(serialNo)
        {
            Battery = battery;
        }

        public int Battery { get; }
    }
}
