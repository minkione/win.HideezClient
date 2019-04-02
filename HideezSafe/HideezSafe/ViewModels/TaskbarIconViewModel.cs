using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Mvvm;
using MvvmExtentions.Commands;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media;

namespace HideezSafe.ViewModels
{
    /// <summary>
    /// A ViewModel class for taskbar icon in the MVVM pattern.
    /// </summary>
    class TaskbarIconViewModel : LocalizedObject
    {
        private readonly IMenuFactory menuFactory;
        private readonly IWindowsManager windowsManager;

        #region Property

        private readonly string appName = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;
        private string toolTip;

        /// <summary>
        ///  A tooltip text that is being displayed.
        /// </summary>
        [Localization]
        public string ToolTip
        {
            get
            {
                string info = L(toolTip);
                return appName + (string.IsNullOrEmpty(info) ? "" : $": {info}");
            }
            set { Set(ref toolTip, value); }
        }

        private ImageSource iconSource;

        public ImageSource IconSource
        {
            get { return iconSource; }
            set { Set(ref iconSource, value); }
        }

        public ObservableCollection<MenuItemViewModel> MenuItems { get; } = new ObservableCollection<MenuItemViewModel>();

        #endregion Property

        #region Command

        /// <summary>
        /// Invok by double click by try icon.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x => windowsManager.ActivateMainWindow(),
                };
            }
        }

        #endregion Command

        public TaskbarIconViewModel(IMenuFactory menuFactory, IWindowsManager windowsManager)
        {
            this.menuFactory = menuFactory;
            this.windowsManager = windowsManager;

            InitMenuItems();
        }

        private void InitMenuItems()
        {
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.ShowWindow));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Separator));
            // MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.AddDevice));
            // MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.ChangePassword));
            // MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Separator));
            // MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Lenguage));
            // MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.CheckForUpdates));
            // MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.LaunchOnStartup));
            // MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Separator));
            // MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.UserManual));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.TechnicalSupport));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.LiveChat));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Legal));
            // MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Separator));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.RFIDUsage));
            // MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.VideoTutorial));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Separator));
            // MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.LogOff));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Exit));
        }
    }
}
