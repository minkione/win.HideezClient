using System;
using System.Threading.Tasks;
using System.Windows;

namespace HideezClient.Modules
{
    public class NotificationOptions
    {
        public static TimeSpan LongTimeout { get { return TimeSpan.FromSeconds(15); } } 
        public static TimeSpan DefaultTimeout { get { return TimeSpan.FromSeconds(7); } }


        public NotificationPosition Position { get; set; } = NotificationPosition.Normal;
        /// <summary>
        /// Close notification when window notificatiuon container deactivate
        /// </summary>
        public bool CloseWhenDeactivate { get; set; }
        public bool SetFocus { get; set; }
        public TimeSpan CloseTimeout { get; set; } = DefaultTimeout;
        public TaskCompletionSource<bool> TaskCompletionSource { get; set; }

    }
}
