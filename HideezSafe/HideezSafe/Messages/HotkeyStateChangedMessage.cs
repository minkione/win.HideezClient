using HideezSafe.Models;
using HideezSafe.Modules.HotkeyManager;

namespace HideezSafe.Messages
{
    class HotkeyStateChangedMessage
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
