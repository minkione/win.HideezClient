using System;
using System.Threading.Tasks;
using System.Windows;

namespace HideezClient.Modules
{
    public class NotificationOptions
    {
        public static TimeSpan LongTimeout { get { return TimeSpan.FromSeconds(15); } } 
        public static TimeSpan DefaultTimeout { get { return TimeSpan.FromSeconds(7); } }
        public static TimeSpan NoTimeout { get { return TimeSpan.Zero; } }

        public NotificationPosition Position { get; set; } = NotificationPosition.Normal;
        /// <summary>
        /// Close notification when window notificatiuon container deactivate
        /// </summary>
        public bool CloseWhenDeactivate { get; set; }
        public bool SetFocus { get; set; }
        /// <summary>
        /// Replace notification with same Id
        /// </summary>
        public bool IsReplace { get; set; } = true;
        public TimeSpan CloseTimeout { get; set; } = DefaultTimeout;
        public TaskCompletionSource<bool> TaskCompletionSource { get; set; }

    }
}
