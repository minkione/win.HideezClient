using HideezClient.Models;

namespace HideezClient.Messages
{
    class ButtonPressedMessage
    {
        public ButtonPressedMessage(string deviceId, UserAction action)
        {
            DeviceId = deviceId;
            Action = action;
        }

        public string DeviceId { get; }

        public UserAction Action { get; }
    }
}
