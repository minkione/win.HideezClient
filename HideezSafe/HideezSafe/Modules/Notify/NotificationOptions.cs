using System;
using System.Windows;

namespace HideezSafe.Modules
{
    public class NotificationOptions
    {
        public NotificationPosition Position { get; set; } = NotificationPosition.Normal;
        public bool CloseWhenLostFocus { get; set; }
        public bool SetFocus { get; set; } = true;
        public TimeSpan CloseTimeout { get; set; } = TimeSpan.FromSeconds(15);
    }
}
