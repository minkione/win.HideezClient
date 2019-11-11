using Hideez.SDK.Communication.Device;

namespace HideezClient.Messages.Remote
{
    class Remote_DeviceStateChangedMessage : Remote_BaseMessage
    {
        public DeviceState State { get; }

        public Remote_DeviceStateChangedMessage(string id, DeviceState state) 
            : base(id)
        {
            State = state;
        }
    }
}
