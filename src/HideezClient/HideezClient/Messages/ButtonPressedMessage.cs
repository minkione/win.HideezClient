using Hideez.SDK.Communication;
using HideezClient.Models;
using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public class ButtonPressedMessage: PubSubMessageBase
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
