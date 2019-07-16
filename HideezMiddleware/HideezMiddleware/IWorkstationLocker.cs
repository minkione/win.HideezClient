namespace HideezMiddleware
{
    public interface IWorkstationLocker
    {
        bool IsEnabled { get; }

        void Start();
        void Stop();

        void LockWorkstation();
    }
}
