using Hideez.SDK.Communication;
using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public class DeviceButtonPressedMessage: PubSubMessageBase
    {
        public DeviceButtonPressedMessage(string deviceId, ButtonPressCode button)
        {
            DeviceId = deviceId;
            Button = button;
        }

        public string DeviceId { get; }

        public ButtonPressCode Button { get; }
    }
}
