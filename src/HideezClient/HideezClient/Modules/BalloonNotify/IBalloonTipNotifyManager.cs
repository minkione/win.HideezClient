namespace HideezClient.Modules
{
    interface IBalloonTipNotifyManager
    {
        void ShowError(string title, string description);
        void ShowInfo(string title, string description);
        void ShowWarning(string title, string description);
    }
}
