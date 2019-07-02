using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Models.Settings;
using HideezSafe.Modules.SettingsManager;
using HideezSafe.Modules.Localize;
using HideezSafe.Properties;
using HideezSafe.Utilities;
using HideezSafe.ViewModels;
using MvvmExtensions.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using NLog;
using System.IO;
using NLog.Layouts;
using System.Threading.Tasks;
using HideezSafe.Models;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.HideezServiceReference;
using System.ServiceModel;

namespace HideezSafe.Modules
{
    class MenuFactory : IMenuFactory
    {
        private readonly IMessenger messenger;
        private readonly IStartupHelper startupHelper;
        private readonly IWindowsManager windowsManager;
        private readonly IAppHelper appHelper;
        private readonly ISettingsManager<ApplicationSettings> settingsManager;
        private readonly ISupportMailContentGenerator supportMailContentGenerator;
        private readonly IServiceProxy serviceProxy;

        public MenuFactory(IMessenger messenger, IStartupHelper startupHelper
            , IWindowsManager windowsManager, IAppHelper appHelper,
            ISettingsManager<ApplicationSettings> settingsManager, ISupportMailContentGenerator supportMailContentGenerator,
            IServiceProxy serviceProxy)
        {
            this.messenger = messenger;
            this.startupHelper = startupHelper;
            this.windowsManager = windowsManager;
            this.appHelper = appHelper;
            this.settingsManager = settingsManager;
            this.supportMailContentGenerator = supportMailContentGenerator;
            this.serviceProxy = serviceProxy;
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
                    return GetViewModel("Menu.TechnicalSupport", x => Task.Run(() => OnTechSupportAsync("SupportMail")));
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
                    return GetLanguages();
                case MenuItemType.LaunchOnStartup:
                    return GetLaunchOnStartup();
                case MenuItemType.GetLogsSubmenu:
                    return GetLogsSubmenu();
                case MenuItemType.Separator:
                    return null;
                default:
                    Debug.Assert(false, $"The type: {type} of menu is not supported.");
                    return null;
            }
        }

        public MenuItemViewModel GetMenuItem(Device device, MenuItemType type)
        {
            if(device == null)
            {
                Debug.Assert(false, "Device can not be null.");
            }

            switch (type)
            {
                case MenuItemType.AddCredential:
                    return GetMenuAddCredential(device);
                case MenuItemType.DisconnectDevice:
                    return GetMenuDisconnectDevice(device);
                case MenuItemType.RemoveDevice:
                    return GetMenuRemoveDevice(device);
                default:
                    Debug.Assert(false, $"The type: {type} of menu is not supported.");
                    return null;
            }
        }

        private MenuItemViewModel GetMenuAddCredential(Device device)
        {
            return new MenuItemViewModel
            {
                Header = "Menu.SavePCPassword",
                Command = new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        if (x is Device d)
                        {
                            windowsManager.ShowDialogAddCredential(d);
                        }
                    },
                    CanExecuteFunc = () => device.IsConnected
                },
                CommandParameter = device,
            };
        }

        private MenuItemViewModel GetMenuRemoveDevice(Device device)
        {
            return new MenuItemViewModel
            {
                Header = "Menu.RemoveDevice",
                Command = new DelegateCommand
                {
                    CommandAction = OnRemoveDevice,
                },
                CommandParameter = device,
            };
        }

        private MenuItemViewModel GetMenuDisconnectDevice(Device device)
        {
            return new MenuItemViewModel
            {
                Header = "Menu.DisconnectDevice",
                Command = new DelegateCommand
                {
                    CommandAction = OnDisconnectDevice,
                    CanExecuteFunc = () => device.IsConnected
                },
                CommandParameter = device,
            };
        }

        private async void OnDisconnectDevice(object paran)
        {
            if (paran is Device device)
            {
                try
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to disconnect {device.Name}?",
                        $"Disconnect {device.Name}",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                        await serviceProxy.GetService().DisconnectDeviceAsync(device.Id);
                }
                catch (FaultException<HideezServiceFault> ex)
                {
                    windowsManager.ShowError(ex.Message);
                }
                catch (Exception ex)
                {
                    windowsManager.ShowError(ex.Message);
                }
            }
        }

        private async void OnRemoveDevice(object paran)
        {
            if (paran is Device device)
            {
                try
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to remove {device.Name}?{Environment.NewLine}Note: All manually stored data will be lost!",
                        $"Remove {device.Name}",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                        await serviceProxy.GetService().RemoveDeviceAsync(device.Id);
                }
                catch (FaultException<HideezServiceFault> ex)
                {
                    windowsManager.ShowError(ex.Message);
                }
                catch (Exception ex)
                {
                    windowsManager.ShowError(ex.Message);
                }
            }
        }

        private async Task OnTechSupportAsync(string techSupportUriKey)
        {
            string techSupportUri = TranslationSource.Instance[techSupportUriKey];
            var mail = await supportMailContentGenerator.GenerateSupportMail(techSupportUri);
            appHelper.OpenUrl(mail);
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

        private MenuItemViewModel GetLanguages()
        {
            var menuLenguage = new MenuItemViewModel { Header = "Menu.InterfaceLanguages", };
            menuLenguage.MenuItems = new ObservableCollection<MenuItemViewModel>();

            // supported cultures
            CultureInfo defaultCulture = new CultureInfo(settingsManager.Settings.SelectedUiLanguage);
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

        private MenuItemViewModel GetLogsSubmenu()
        {
            var logsMenu = new MenuItemViewModel { Header = "Menu.Logs", };
            logsMenu.MenuItems = new ObservableCollection<MenuItemViewModel>();

            try
            {
                var openClientLogsFolderItem = new MenuItemViewModel
                {
                    Header = "Menu.Logs.OpenClientFolder",
                    Command = new DelegateCommand
                    {
                        CommandAction = x =>
                        {
                            try
                            {
                                var logsPath = LogManager.Configuration.Variables["logDir"].Text;
                                var fullPath = LogManagement.GetTargetFolder(logsPath);
                                Process.Start(fullPath);
                            }
                            catch (Exception) { }
                        }
                    }
                };
                var openServiceLogsFolderItem = new MenuItemViewModel
                {
                    Header = "Menu.Logs.OpenServiceFolder",
                    Command = new DelegateCommand
                    {
                        CommandAction = x =>
                        {
                            try
                            {
                                var logsPath = LogManager.Configuration.Variables["serviceLogDir"].Text;
                                var fullPath = LogManagement.GetTargetFolder(logsPath);
                                Process.Start(fullPath);
                            }
                            catch (Exception) { }
                        }
                    }
                };

                logsMenu.MenuItems.Add(openClientLogsFolderItem);
                logsMenu.MenuItems.Add(openServiceLogsFolderItem);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Debug.Assert(false);
            }

            return logsMenu;
        }
    }
}
