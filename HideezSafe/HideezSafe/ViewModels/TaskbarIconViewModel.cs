using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Modules;
using HideezSafe.Mvvm;
using HideezSafe.Mvvm.Messages;
using MvvmExtentions.Commands;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;

namespace HideezSafe.ViewModels
{
    class TaskbarIconViewModel : ObservableObject
    {
        private readonly IMenuFactory menuFactory;
        private readonly IMessenger messenger;

        #region Property

        private string toolTip = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;

        public string ToolTip
        {
            get { return toolTip; }
            set { Set(ref toolTip, value); }
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
                    CommandAction = x => messenger.Send(new ActivateWindowMessage()),
                };
            }
        }

        #endregion Command
        public TaskbarIconViewModel(IMenuFactory menuFactory, IMessenger messenger)
        {
            this.menuFactory = menuFactory;
            this.messenger = messenger;

            InitMenuItems();
        }

        private void InitMenuItems()
        {
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.ShowWindow));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Separator));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.AddDevice));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.ChangePassword));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Separator));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Lenguage));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.CheckForUpdates));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.LaunchOnStartup));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Separator));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.UserManual));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.TechnicalSupport));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.LiveChat));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Legal));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Separator));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.RFIDUsage));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.VideoTutorial));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Separator));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.LogOff));
            MenuItems.Add(menuFactory.GetMenuItem(MenuItemType.Exit));
        }
    }
}
