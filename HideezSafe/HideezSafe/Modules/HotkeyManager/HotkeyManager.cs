using System;
using System.Windows.Input;
using HideezSafe.Modules.SettingsManager;
using HideezSafe.Models.Settings;
using HideezSafe.Models;
using NHotkey;
using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Messages;
using NLog;

namespace HideezSafe.Modules.HotkeyManager
{
    /// <summary>
    /// Responsible for managing hotkeys registration, notification about their status and hotkey input notification
    /// </summary>
    internal sealed class HotkeyManager : IHotkeyManager
	{
        private readonly Logger log = LogManager.GetCurrentClassLogger();
		private readonly KeyGestureConverter keyGestureConverter = new KeyGestureConverter();
		private readonly ISettingsManager<HotkeySettings> hotkeySettingsManager;
        private readonly IMessenger messenger;
        private bool enabled = false;

		public HotkeyManager(ISettingsManager<HotkeySettings> hotkeySettingsManager, IMessenger messanger)
		{
			this.hotkeySettingsManager = hotkeySettingsManager;
            this.messenger = messanger;

            messenger?.Register<SettingsChangedMessage<HotkeySettings>>(this, OnHotkeysSettingsChanged);
		}

        /// <summary>
        /// If set to true, the available hotkeys will be registered by manager. 
        /// If set to false, the hotkeys will be unregistered and may be claimed by other applications.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    OnManagerEnabledChanged(value);
                }
            }
        }

        private void OnHotkeysSettingsChanged(SettingsChangedMessage<HotkeySettings> message)
        {
            if (Enabled)
            {
                UnsubscribeAllHotkeys();
                SubscribeAllHotkeys();
            }
        }

        private void OnManagerEnabledChanged(bool newValue)
        {
            if (newValue)
            {
                UnsubscribeAllHotkeys();
                SubscribeAllHotkeys();
            }
            else
            {
                UnsubscribeAllHotkeys();
            }
        }

        /// <summary>
        /// Returns hotkey combination registered for specified action
        /// </summary>
        /// <param name="action">Hotkey action</param>
        /// <returns>Key combination if hotkey is registered to action in settings. Otherwise returns empty string.</returns>
		public string GetGetHotkeyForAction(UserAction action)
		{
            hotkeySettingsManager.GetSettingsAsync().Result.Hotkeys.TryGetValue(action, out string hotkey);
			return hotkey ?? string.Empty;
		}

        /// <summary>
        /// Check if the specified key combination is taken by other application. 
        /// </summary>
        /// <param name="action">Action registered for hotkey</param>
        /// <param name="hotkey">String representation of keys combination</param>
        /// <returns>Returns true if the specified hotkey can be registered. Otherwise returns false.</returns>
        public bool IsFreeHotkey(UserAction action, string hotkey)
		{
            if (string.IsNullOrEmpty(hotkey))
                return true;

            try
            {
                if (Enabled)
                    UnsubscribeAllHotkeys();

                try
                {
                    KeyGesture kg = ConvertStringKeysToKeyGesture(hotkey);
                    NHotkey.Wpf.HotkeyManager.Current.AddOrReplace("_hideez_test_", kg.Key, kg.Modifiers, (o, e) => { });
                    NHotkey.Wpf.HotkeyManager.Current.Remove("_hideez_test_");
                    return true;
                }
                catch (Exception) // If an exception occures, it means that hotkey is already taken
                {
                    return false;
                }
            }
            finally
            {
                if (Enabled)
                    SubscribeAllHotkeys();
            }
		}

        /// <summary>
        /// Check if the specified hotkey is already used for another action in our application
        /// </summary>
        /// <param name="action">Action registered for hotkey</param>
        /// <param name="hotkey">String representation of keys combination</param>
        /// <returns>Returns true if hotkey is used only once in current application. Otherwise returns false.</returns>
        public bool IsUniqueHotkey(UserAction action, string hotkey)
        {
            foreach (var h in hotkeySettingsManager.GetSettingsAsync().Result.Hotkeys)
            {
                if (h.Key != action && h.Value == hotkey)
                    return false;
            }

            return true;
        }

		private void SubscribeAllHotkeys()
		{
			foreach (var shortcut in hotkeySettingsManager.GetSettingsAsync().Result.Hotkeys)
			{
				SubscribeHotkey(shortcut.Key, shortcut.Value);
			}
		}

		private void UnsubscribeAllHotkeys()
		{
			foreach (var item in hotkeySettingsManager.GetSettingsAsync().Result.Hotkeys)
			{
				UnsubscribeHotkey(item.Key);
			}
		}

		private void SubscribeHotkey(UserAction action, string hotkey)
		{
			try
			{
				if (!string.IsNullOrEmpty(hotkey))
				{
					KeyGesture kg = ConvertStringKeysToKeyGesture(hotkey);
					NHotkey.Wpf.HotkeyManager.Current.AddOrReplace(Enum.GetName(typeof(UserAction), action), kg.Key, kg.Modifiers, this.OnHotkeyInput);

                    messenger?.Send(new HotkeyStateChangedMessage(action, hotkey, HotkeyState.Subscribed));
                }
            }
			catch (Exception ex)
			{
                log.Error(ex.Message);

                messenger?.Send(new HotkeyStateChangedMessage(action, hotkey, HotkeyState.Unavailable));
			}
		}

		private void UnsubscribeHotkey(UserAction action)
		{
			try
			{
				NHotkey.Wpf.HotkeyManager.Current.Remove(Enum.GetName(typeof(UserAction), action));

                messenger?.Send(new HotkeyStateChangedMessage(action, GetGetHotkeyForAction(action), HotkeyState.Unavailable));
            }
            catch (Exception ex)
			{
                log.Error(ex.Message);

                messenger?.Send(new HotkeyStateChangedMessage(action, GetGetHotkeyForAction(action), HotkeyState.Unavailable));
            }
        }

		/// <summary>
		/// Convert from string to KeyGesture
		/// </summary>
		/// <param name="keys">
		/// Hot key 
		/// </param>
		private KeyGesture ConvertStringKeysToKeyGesture(string keys)
		{
			return (KeyGesture)keyGestureConverter.ConvertFromString(keys);
		}

		private void OnHotkeyInput(object sender, HotkeyEventArgs e)
		{
			if (Enum.TryParse<UserAction>(e.Name, out UserAction action) 
                && hotkeySettingsManager.GetSettingsAsync().Result.Hotkeys.TryGetValue(action, out string hotkey))
			{
                messenger?.Send(new HotkeyPressedMessage(action, hotkey));
			}
		}
	}
}
