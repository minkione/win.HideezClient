namespace HideezSafe.Modules
{
    interface INotifier
    {
        void ShowInfo(string title, string message, NotificationOptions options = null);
        void ShowWarn(string title, string message, NotificationOptions options = null);
        void ShowError(string title, string message, NotificationOptions options = null);
    }
}