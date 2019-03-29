using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Properties;
using HideezSafe.Utilities;
using HideezSafe.ViewModels;
using MvvmExtensions.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace HideezSafe.Modules
{
    class MenuFactory : IMenuFactory
    {
        private readonly IMessenger messenger;
        private readonly IStartupHelper startupHelper;
        private readonly IWindowsManager windowsManager;
        private readonly IAppHelper appHelper;

        public MenuFactory(IMessenger messenger, IStartupHelper startupHelper
            , IWindowsManager windowsManager, IAppHelper appHelper)
        {
            this.messenger = messenger;
            this.startupHelper = startupHelper;
            this.windowsManager = windowsManager;
            this.appHelper = appHelper;
        }

        public MenuItemViewModel GetMenuItem(MenuItemType type)
        {
            switch (type)
            {
                case MenuItemType.ShowWindow:
                    return GetViewModel("Menu.ShowWindow", x => windowsManager.ActivateMainWindow());
                case MenuItemType.AddDevice:
                    return GetViewModel("Menu.AddDevice", x => throw new NotImplementedException());
                case MenuItemType.CheckForUpdates:
                    return GetViewModel("Menu.CheckForUpdates", x => throw new NotImplementedException());
                case MenuItemType.ChangePassword:
                    return GetViewModel("Menu.ChangePassword", x => throw new NotImplementedException());
                case MenuItemType.UserManual:
                    return GetViewModel("Menu.UserManual", x => OnOpenUrl("Url.UserManual"));
                case MenuItemType.TechnicalSupport:
                    return GetViewModel("Menu.TechnicalSupport", x => throw new NotImplementedException());
                case MenuItemType.LiveChat:
                    return GetViewModel("Menu.LiveChat", x => OnOpenUrl("Url.LiveChat"));
                case MenuItemType.Legal:
                    return GetViewModel("Menu.Legal", x => OnOpenUrl("Url.Legal"));
                case MenuItemType.RFIDUsage:
                    return GetViewModel("Menu.RFIDUsage", x => OnOpenUrl("Url.RFIDUsage"));
                case MenuItemType.VideoTutorial:
                    return GetViewModel("Menu.VideoTutorial", x => OnOpenUrl("Url.VideoTutorial"));
                case MenuItemType.LogOff:
                    return GetViewModel("Menu.LogOff", x => throw new NotImplementedException());
                case MenuItemType.Exit:
                    return GetViewModel("Menu.Exit", x => appHelper.Shutdown());
                case MenuItemType.Lenguage:
                    return GetLenguages();
                case MenuItemType.LaunchOnStartup:
                    return GetLaunchOnStartup();
                case MenuItemType.Separator:
                default:
                    return null;
            }
        }

        private void OnOpenUrl(string urlKey)
        {
            string url = TranslationSource.Instance[urlKey];
            appHelper.OpenUrl(url);
        }

        private MenuItemViewModel GetLaunchOnStartup()
        {
            MenuItemViewModel vm = GetViewModel("Menu.LaunchOnStartup", x => startupHelper.ReverseState());
            vm.IsCheckable = true;
            vm.CommandParameter = vm;
            vm.IsChecked = startupHelper.IsInStartup();
            // TODO add weak event handler
            startupHelper.StateChanged += (appName, state) => vm.IsChecked = (state == AutoStartupState.On);
            return vm;
        }

        private MenuItemViewModel GetLenguages()
        {
            var menuLenguage = new MenuItemViewModel { Header = "Menu.InterfaceLanguages", };
            menuLenguage.MenuItems = new ObservableCollection<MenuItemViewModel>();

            // supported cultures
            CultureInfo defaultCulture = Settings.Default.Culture;
            foreach (CultureInfo culture in TranslationSource.Instance.SupportedCultures)
            {
                try
                {
                    char[] arrName = culture.NativeName.ToCharArray();
                    arrName[0] = Char.ToUpper(arrName[0], culture);

                    var menuItem = new LanguageMenuItemViewModel
                    {
                        Header = new string(arrName),
                        IsCheckable = true,
                        IsChecked = culture.Equals(defaultCulture),
                        CommandParameter = culture,
                    };
                    menuItem.Command = new DelegateCommand
                    {
                        CommandAction = x =>
                        {
                            if (x is CultureInfo cultureInfo)
                                OnApplyLanguage(cultureInfo);
                        }
                    };
                    menuLenguage.MenuItems.Add(menuItem);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    Debug.Assert(false);
                }
            }

            return menuLenguage;
        }

        private void OnApplyLanguage(CultureInfo cultureInfo)
        {
            appHelper.ChangeCulture(cultureInfo);
        }

        private MenuItemViewModel GetViewModel(string header, Action<object> action)
        {
            return new MenuItemViewModel
            {
                Header = header,
                Command = new DelegateCommand
                {
                    CommandAction = action,
                }
            };
        }
    }
}
