namespace HideezMiddleware.ScreenActivation
{
    public interface IScreenActivator
    {
        void ActivateScreen();

        void StartPeriodicScreenActivation(int timeout = 30_000);

        void StopPeriodicScreenActivation();
    }
}
