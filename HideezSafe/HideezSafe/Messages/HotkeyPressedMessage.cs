using HideezSafe.Models;

namespace HideezSafe.Messages
{
    class HotkeyPressedMessage
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
