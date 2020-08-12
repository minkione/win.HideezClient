using HideezClient.Models;
using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public class HotkeyPressedMessage: PubSubMessageBase
    {
        public HotkeyPressedMessage(UserAction action, string hotkey)
        {
            Action = action;
            Hotkey = hotkey;
        }

        public UserAction Action { get; set; }

        public string Hotkey { get; set; }
    }
}
