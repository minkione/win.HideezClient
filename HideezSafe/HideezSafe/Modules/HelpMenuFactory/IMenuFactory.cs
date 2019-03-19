using HideezSafe.ViewModels;

namespace HideezSafe.Modules
{
    public enum MenuItemType
    {
        ShowWindow,
        AddDevice,
        CheckForUpdates,
        ChangePassword,
        UserManual,
        TechnicalSupport,
        LiveChat,
        Legal,
        RFIDUsage,
        VideoTutorial,
        LogOff,
        Exit,
        Lenguage,
        Separator,
        LaunchOnStartup,
    }

    interface IMenuFactory
    {
        MenuItemViewModel GetMenuItem(MenuItemType type);
    }
}
