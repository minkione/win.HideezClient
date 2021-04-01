using HideezClient.Messages.Hotkeys;
using HideezClient.Models;
using HideezClient.Models.Settings;
using HideezClient.Modules;
using HideezMiddleware.Settings;
using MahApps.Metro.Controls;
using Meta.Lib.Modules.PubSub;
using MvvmExtensions.Commands;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HideezClient.ViewModels
{
    class HotkeySettingsViewModel: ReactiveObject
    {
        private readonly ISettingsManager<HotkeySettings> _settingsManager;
        private readonly IWindowsManager _windowsManager;
        private readonly IMetaPubSub _metaMessanger;

        public HotkeySettingsViewModel(ISettingsManager<HotkeySettings> settingsManager, IWindowsManager windowsManager, IMetaPubSub metaMessanger)
        {
            _settingsManager = settingsManager;
            _windowsManager = windowsManager;
            _metaMessanger = metaMessanger;

            settingsManager.SettingsChanged += SettingsManager_SettingsChanged;

            UpdateHotkeys(settingsManager.Settings.Hotkeys);
        }

        public ObservableCollection<HotkeyViewModel> Hotkeys { get; } = new ObservableCollection<HotkeyViewModel>();

        #region Commands
        public ICommand AddHotkeyCommand
        {
            get => new DelegateCommand
            {
                CommandAction = x =>
                {
                    Task.Run(OnAddHotkey);
                }
            };
        }

        public ICommand ResetToDefaultCommand
        {
            get => new DelegateCommand
            {
                CommandAction = x =>
                {
                    Task.Run(ResetToDefault);
                }
            };
        }
        #endregion

        async Task OnAddHotkey()
        {
            await _metaMessanger.Publish(new AddHotkeyMessage(string.Empty, UserAction.None));
        }

        async Task ResetToDefault()
        {
            var result = await _windowsManager.ShowResetToDefaultHotkeysAsync();
            if (result)
                _settingsManager.SaveSettings(new HotkeySettings());
        }

        void SettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<HotkeySettings> e)
        {
            UpdateHotkeys(e.NewSettings.Hotkeys);
        }

        void UpdateHotkeys(Hotkey[] hotkeys)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Hotkeys.Clear();

                foreach (var hotkey in hotkeys)
                {
                    Hotkeys.Add(new HotkeyViewModel(hotkey, _metaMessanger));
                }
            });
        }
    }
}
