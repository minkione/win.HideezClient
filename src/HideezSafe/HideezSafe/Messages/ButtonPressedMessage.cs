using HideezSafe.Models;

namespace HideezSafe.Messages
{
    class ButtonPressedMessage
    {
        public ButtonPressedMessage(string deviceId, UserAction action, int number)
        {
            DeviceId = deviceId;
            Action = action;
            Number = number;
        }

        public string DeviceId { get; }

        public UserAction Action { get; }

        public int Number { get; }
    }
}
