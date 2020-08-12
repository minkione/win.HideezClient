using HideezClient.Models;
using HideezClient.Modules.HotkeyManager;
using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public class HotkeyStateChangedMessage: PubSubMessageBase
    {
        public HotkeyStateChangedMessage(UserAction action, string hotkey, HotkeyState state)
        {
            Action = action;
            Hotkey = hotkey;
            State = state;
        }

        public UserAction Action { get; set; }

        public string Hotkey { get; set; }

        public HotkeyState State { get; set; }
    }
}
