using Hideez.SDK.Communication;
using HideezClient.Models;

namespace HideezClient.Messages
{
    class ButtonPressedMessage
    {
        public ButtonPressedMessage(string deviceId, UserAction action, ButtonPressCode code)
        {
            DeviceId = deviceId;
            Action = action;
            Code = code;
        }

        public string DeviceId { get; }

        public UserAction Action { get; }

        public ButtonPressCode Code { get; }
    }
}
