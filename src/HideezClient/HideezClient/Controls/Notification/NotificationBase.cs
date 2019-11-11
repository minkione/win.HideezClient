using HideezClient.Modules;
using MahApps.Metro.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HideezClient.Controls
{
    public abstract class NotificationBase : UserControl
    {
        private readonly object lockObj = new object();
        private bool closing = false;
        private DispatcherTimer timer;

        protected NotificationBase(NotificationOptions options)
        {
            Options = options;
            Loaded += OnLoaded;
        }

        public event EventHandler Closed;

        public NotificationOptions Options { get; }

        public NotificationPosition Position
        {
            get
            {
                return Options.Position;
            }
        }

        public void ResetCloseTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Start();
            }
        }

        public void Close()
        {
            lock (lockObj)
            {
                if (!closing)
                {
                    Options.TaskCompletionSource?.TrySetResult(false);

                    closing = true;
                    BeginAnimation("HideNotificationAnimation");

                    Task.Run(async () =>
                    {
                        await Task.Delay(((Duration)FindResource("AnimationHideTime")).TimeSpan);
                        await App.Current.Dispatcher.InvokeAsync(() => Closed?.Invoke(this, EventArgs.Empty));
                    });
                }
            }
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

            if (Options.SetFocus)
            {
                Focusable = true;
                Focus();
            }

            StartTimer(Options.CloseTimeout);
        }

        protected void StartTimer(TimeSpan time)
        {
            if (time != TimeSpan.Zero && timer == null)
            {
                timer = new DispatcherTimer
                {
                    Interval = time,
                };

                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            timer?.Stop();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            timer?.Start();
        }

        private void Timer_Tick(object s, EventArgs e)
        {
            Options.TaskCompletionSource?.TrySetException(new TimeoutException("Close notification by timeout."));
            timer.Tick -= Timer_Tick;
            timer.Stop();
            Close();
        }
    }
}
