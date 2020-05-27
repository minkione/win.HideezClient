namespace DeviceMaintenance.Messages
{
    public class ConnectDeviceCommand : MessageBase
    {
        public string Mac { get; internal set; }

        public ConnectDeviceCommand(string mac)
        {
            Mac = mac;
        }
    }
}
