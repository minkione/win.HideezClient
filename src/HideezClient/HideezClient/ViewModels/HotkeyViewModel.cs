using HideezClient.Messages.Hotkeys;
using HideezClient.Models;
using HideezClient.Utilities;
using MahApps.Metro.Controls;
using Meta.Lib.Modules.PubSub;
using MvvmExtensions.Commands;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HideezClient.ViewModels
{
    class HotkeyViewModel: ReactiveObject
    {
        public class HotkeyActionOption
        {
            public string Title { get; set; }

            public UserAction Action { get; set; }
        }

        private readonly Hotkey _hotkeyModel;
        private readonly IMetaPubSub _metaMessenger;

        DelayedMethodCaller dmc = new DelayedMethodCaller(2000);

        public HotkeyViewModel(Hotkey hotkeyModel, IMetaPubSub metaMessenger)
        {
            _hotkeyModel = hotkeyModel;
            _metaMessenger = metaMessenger;

            HotkeyActionOptions = new List<HotkeyActionOption>
            {
                new HotkeyActionOption { Action = UserAction.None, Title = UserAction.None.ToString()},
                new HotkeyActionOption { Action = UserAction.AddAccount, Title = UserAction.AddAccount.ToString()},
                new HotkeyActionOption { Action = UserAction.InputLogin, Title = UserAction.InputLogin.ToString()},
                new HotkeyActionOption { Action = UserAction.InputOtp, Title = UserAction.InputOtp.ToString()},
                new HotkeyActionOption { Action = UserAction.InputPassword, Title = UserAction.InputPassword.ToString()},
                new HotkeyActionOption { Action = UserAction.LockWorkstation, Title = UserAction.LockWorkstation.ToString()}
            };

            var actionOption = HotkeyActionOptions.FirstOrDefault(o => o.Action == hotkeyModel.Action);
            if (actionOption == null)
                actionOption = HotkeyActionOptions.FirstOrDefault(o => o.Action == UserAction.None);
            SelectedActionOption = actionOption;
            IsEnabled = hotkeyModel.Enabled;
            Keystroke = hotkeyModel.Keystroke;

            this.ObservableForProperty(vm => vm.IsEnabled).Subscribe(vm => { OnValueChanged(); });
            this.ObservableForProperty(vm => vm.SelectedActionOption).Subscribe(vm => { OnValueChanged(); });
            this.ObservableForProperty(vm => vm.Keystroke).Subscribe(vm => { OnValueChanged(); });
        }

        #region Properties
        [Reactive] public bool IsEnabled { get; set; }
        [Reactive] public List<HotkeyActionOption> HotkeyActionOptions { get; set; }
        [Reactive] public HotkeyActionOption SelectedActionOption { get; set; }
        [Reactive] public string Keystroke { get; set; }
        #endregion

        #region Commands 
        public ICommand DeleteCommand
        {
            get => new DelegateCommand
            {
                CommandAction = x =>
                {
                    Task.Run(DeleteHotkey);
                }
            };
        }
        #endregion

        void OnValueChanged()
        {
            Task.Run(() =>
            {
                dmc.CallMethod(async () =>
                {
                    await _metaMessenger.Publish(new ChangeHotkeyMessage(_hotkeyModel.HotkeyId, IsEnabled, SelectedActionOption.Action, Keystroke));
                });
            });
        }

        async Task DeleteHotkey()
        {
            await _metaMessenger.Publish(new DeleteHotkeyMessage(_hotkeyModel.HotkeyId));
        }

        HotKey GetHotKeyFromString(string keystroke)
        {
            var keysStrings = keystroke.Split('+').ToList();

            ModifierKeys modifierKeys = ModifierKeys.None;
            ModifierKeysConverter modifierKeysConverter = new ModifierKeysConverter();

            Key key = Key.None;
            KeyConverter keyConverter = new KeyConverter();

            foreach(var keyString in keysStrings)
            {
                try
                {
                    ModifierKeys modifierKey = (ModifierKeys)modifierKeysConverter.ConvertFromString(keyString);
                    int number = (int)modifierKeys + (int)modifierKey;
                    modifierKeys = (ModifierKeys)number;
                }
                catch(Exception ex)
                {
                    key = (Key)keyConverter.ConvertFromString(keyString);
                }
            }

            return new HotKey(key, modifierKeys);
        }
    }
}
