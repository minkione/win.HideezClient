namespace HideezSafe.Modules
{
    interface IWorkstationManager
    {
        void ForceShutdown();
        void LockPC();
        void ActivateScreen();
    }
}