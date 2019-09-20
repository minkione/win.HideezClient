using System;
using System.Timers;

namespace HideezClient.Utilities
{
    class DelayedAction
    {
        private Timer timer;
        private Action action;
        public DelayedAction(Action action, int delay)
        {
            this.action = action;
            timer = new Timer(delay);
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => action();
        }

        public void RunDelayedAction()
        {
            timer.Stop();
            timer.Start();
        }
    }
}
