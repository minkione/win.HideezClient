using HideezSafe.Modules;
using MahApps.Metro.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HideezSafe.Controls
{
    public abstract class NotificationBase : UserControl
    {
        private readonly object lockObj = new object();
        private bool closing = false;
        private bool result = false;
        private DispatcherTimer timer;

        protected NotificationBase(NotificationOptions options)
        {
            Options = options;
            Loaded += OnLoaded;
        }

        public event EventHandler Closed;
        public event EventHandler Closing;

        public bool Result
        {
            get { return result; }
            protected set
            {
                result = value;
                Close();
            }
        }

        public NotificationOptions Options { get; }

        public NotificationPosition Position
        {
            get
            {
                return Options.Position;
            }
        }

        public void Close()
        {
            lock (lockObj)
            {
                if (!closing)
                {
                    Closing?.Invoke(this, EventArgs.Empty);

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

            if (Options.CloseTimeout != TimeSpan.Zero && timer == null)
            {
                timer = new DispatcherTimer
                {
                    Interval = Options.CloseTimeout
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
