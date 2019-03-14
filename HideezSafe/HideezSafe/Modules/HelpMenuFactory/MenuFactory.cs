using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Mvvm.Messages;
using HideezSafe.Properties;
using HideezSafe.Utilities;
using HideezSafe.ViewModels;
using MvvmExtentions.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace HideezSafe.Modules
{
    class MenuFactory : IMenuFactory
    {
        private readonly IMessenger messenger;
        private readonly IStartupHelper startupHelper;

        public MenuFactory(IMessenger messenger, IStartupHelper startupHelper)
        {
            this.messenger = messenger;
            this.startupHelper = startupHelper;
        }

        public MenuItemViewModel GetMenuItem(MenuItemType type)
        {
            switch (type)
            {
                case MenuItemType.ShowWindow:
                    return GetViewModel("Menu.ShowWindow", x => messenger.Send(new ActivateWindowMessage()));
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
                    return GetViewModel("Menu.Exit", x => messenger.Send(new ShutdownAppMessage()));
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
            messenger.Send(new OpenUrlMessage(url));
        }

        private MenuItemViewModel GetLaunchOnStartup()
        {
            MenuItemViewModel vm = GetViewModel("Menu.LaunchOnStartup", x => OnLaunchOnStartup(x as MenuItemViewModel));
            vm.IsCheckable = true;
            vm.CommandParameter = vm;
            vm.IsChecked = startupHelper.IsInStartup();
            return vm;
        }

        private void OnLaunchOnStartup(MenuItemViewModel viewModel)
        {
            messenger.Send(new InvertStateAutoStartupMessage());
            if (viewModel != null)
                viewModel.IsChecked = startupHelper.IsInStartup();
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

                    var menuItem = GetViewModel(new string(arrName), x =>
                    {
                        if (x is CultureInfo cultureInfo)
                            OnApplyLanguage(cultureInfo, menuLenguage);
                    });
                    menuItem.IsCheckable = true;
                    menuItem.IsChecked = culture.Equals(defaultCulture);
                    menuItem.CommandParameter = culture;
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

        private void OnApplyLanguage(CultureInfo cultureInfo, MenuItemViewModel menuLenguage)
        {
            try
            {
                messenger.Send(new LanguageChangedMessage(cultureInfo));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debug.Assert(false);
            }

            foreach (var menu in menuLenguage.MenuItems)
            {
                if (menu.IsChecked && !menu.Header.Equals(Settings.Default.Culture.NativeName, StringComparison.OrdinalIgnoreCase))
                {
                    menu.IsChecked = false;
                }
            }
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
