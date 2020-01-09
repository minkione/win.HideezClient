using Hideez.SDK.Communication;

namespace HideezClient.Messages
{
    class DeviceButtonPressedMessage
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
