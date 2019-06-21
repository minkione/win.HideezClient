using HideezSafe.Modules;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HideezSafe.Controls
{
    public abstract class NotificationBase : UserControl
    {
        private DispatcherTimer timer;
        private readonly NotificationOptions options;

        protected NotificationBase(NotificationOptions options)
        {
            this.options = options;
            Loaded += OnLoaded;
        }

        public event EventHandler Closed;

        public NotificationPosition Position
        {
            get
            {
                return options != null ? options.Position : NotificationPosition.Normal;
            }
        }

        public void Close()
        {
            BeginAnimation("HideNotificationAnimation");

            Task.Run(async () =>
            {
                await Task.Delay(((Duration)FindResource("AnimationHideTime")).TimeSpan);
                await App.Current.Dispatcher.InvokeAsync(() => Closed?.Invoke(this, EventArgs.Empty));
            });
        }

        private void NotificationBase_LostFocus(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BeginAnimation(string storyboardName)
        {
            if (TryFindResource(storyboardName) is Storyboard storyboard)
            {
                storyboard.Begin(this);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Style = (Style)FindResource("NotificationStyle");
            BeginAnimation("ShowNotificationAnimation");

            if (options.SetFocus)
            {
                Focus();
            }

            if (options.CloseWhenLostFocus)
            {
                LostKeyboardFocus -= NotificationBase_LostFocus;
                LostFocus -= NotificationBase_LostFocus;

                LostKeyboardFocus += NotificationBase_LostFocus;
                LostFocus += NotificationBase_LostFocus;
            }

            if (options.CloseTimeout != TimeSpan.Zero && timer == null)
            {
                timer = new DispatcherTimer
                {
                    Interval = options.CloseTimeout
                };

                timer.Tick += Timer_Tick; ;
                timer.Start();
            }
        }

        private void Timer_Tick(object s, EventArgs e)
        {
            timer.Tick -= Timer_Tick;
            timer.Stop();
            Close();
        }
    }
}
