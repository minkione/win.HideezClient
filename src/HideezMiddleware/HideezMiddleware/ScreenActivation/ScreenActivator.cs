using System;
using System.Timers;

namespace HideezMiddleware.ScreenActivation
{
    public abstract class ScreenActivator : IScreenActivator
    {
        const int ACTIVATION_INTERVAL = 4_000;
        readonly Timer screenActivationTimer;
        readonly Timer timeoutTimer;

        public ScreenActivator()
        {
            screenActivationTimer = new Timer()
            {
                AutoReset = true,
                Interval = ACTIVATION_INTERVAL,
            };
            screenActivationTimer.Elapsed += (e, a) => ActivateScreen();

            timeoutTimer = new Timer()
            {
                AutoReset = false,
            };
            timeoutTimer.Elapsed += (e, a) => StopPeriodicScreenActivation();
        }

        public abstract void ActivateScreen();

        public void StartPeriodicScreenActivation(int timeout = 30000)
        {
            if (timeout < 0)
                throw new ArgumentException("Timeout cannot be less than 0", nameof(timeout));

            StopTimers();

            if (timeout != 0)
            {
                timeoutTimer.Interval = timeout;
                timeoutTimer.Start();
            }

            screenActivationTimer.Start();
        }

        public void StopPeriodicScreenActivation()
        {
            StopTimers();
        }

        void StopTimers()
        {
            screenActivationTimer.Stop();
            timeoutTimer.Stop();
        }
    }
}
