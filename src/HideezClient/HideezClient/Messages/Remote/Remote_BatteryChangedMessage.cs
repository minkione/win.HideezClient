namespace HideezClient.Messages.Remote
{
    class Remote_BatteryChangedMessage : Remote_BaseMessage
    {
        public Remote_BatteryChangedMessage(string serialNo, sbyte battery)
            : base(serialNo)
        {
            Battery = battery;
        }

        public sbyte Battery { get; }
    }
}
