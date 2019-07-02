namespace HideezSafe.Messages.Remote
{
    class Remote_BatteryChangedMessage
    {
        public Remote_BatteryChangedMessage(string serialNo, int battery)
        {
            SerialNo = serialNo;
            Battery = battery;
        }

        public string SerialNo { get; }

        public int Battery { get; }
    }
}
