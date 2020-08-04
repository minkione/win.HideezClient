﻿using System;
using HideezClient.Messages;
using System.Threading.Tasks;
using HideezClient.Modules.Localize;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.Models;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication;
using System.Threading;
using Hideez.ARM;
using HideezClient.Models.Settings;
using HideezMiddleware.Settings;
using HideezClient.Modules.HotkeyManager;

namespace HideezClient.Modules.ActionHandler
{
    /// <summary>
    /// Responsible for receiving user action requests and routing them for execution
    /// </summary>
    class UserActionHandler : Logger
    {
        private readonly IMessenger _messenger;
        private readonly IActiveDevice _activeDevice;
        private readonly InputLogin _inputLogin;
        private readonly InputPassword _inputPassword;
        private readonly InputOtp _inputOtp;
        private int _isPerformingActionLock = 0;
        private const int DELAY_BEFORE_NEXT_ACTION = 500; // Delay after user action is finished but before next one will be handled
        private readonly ISettingsManager<ApplicationSettings> _appSettingsManager;
        private readonly IHotkeyManager _hotkeyManager;
        private readonly IWindowsManager _windowsManager;

        public UserActionHandler(
            IMessenger messenger,
            IActiveDevice activeDevice,
            InputLogin inputLogin,
            InputPassword inputPassword,
            InputOtp inputOtp,
            ISettingsManager<ApplicationSettings> appSettingsManager,
            IHotkeyManager hotkeyManager,
            IWindowsManager windowsManager,
            ILog log)
            : base(nameof(UserActionHandler), log)
        {
            _messenger = messenger;
            _activeDevice = activeDevice;
            _inputLogin = inputLogin;
            _inputPassword = inputPassword;
            _inputOtp = inputOtp;
            _appSettingsManager = appSettingsManager;
            _hotkeyManager = hotkeyManager;
            _windowsManager = windowsManager;

            _messenger.Register<HotkeyPressedMessage>(this, HotkeyPressedMessageHandler);
            _messenger.Register<ButtonPressedMessage>(this, ButtonPressedMessageHandler);
        }

        // Messages from Messenger are events. async void is fine in this case.
        void ButtonPressedMessageHandler(ButtonPressedMessage message)
        {
            Task.Run(async () =>
            {
                WriteLine($"Handling button pressed message ({message.Action.ToString()}, {message.Code.ToString()})");

                if (_activeDevice.Device?.Id == message.DeviceId)
                {
                    if (_activeDevice.Device.OtherConnections > 0)
                    {
                        string info = TranslationSource.Instance["UserAction.ButtonDisabled.ToManyOtherConnections"];
                        var deviceName = _activeDevice.Device.Name;
                        var otherConnections = _activeDevice.Device.OtherConnections;
                        var hotkey = await _hotkeyManager.GetHotkeyForAction(message.Action);
                        var localizedAction = TranslationSource.Instance[$"Enum.UserAction.{message.Action.ToString()}"].ToLowerInvariant();
                        info = string.Format(info, deviceName, otherConnections, hotkey, localizedAction);
                        var msgOptions = new NotificationOptions { CloseTimeout = NotificationOptions.LongTimeout };
                        _messenger.Send(new ShowInfoNotificationMessage(info, options: msgOptions, notificationId: _activeDevice.Device.Mac));
                    }
                    else if (!_activeDevice.Device.IsLoadingStorage)
                        await HandleButtonActionAsync(message.DeviceId, message.Action, message.Code);
                }
                else
                {
                    string warn = string.Format(TranslationSource.Instance["UserAction.DeviceIsNotActive"], message.DeviceId);
                    _messenger.Send(new ShowWarningNotificationMessage(warn, notificationId: _activeDevice.Device?.Mac));
                    WriteLine(warn, LogErrorSeverity.Warning);
                }
            });
        }

        void HotkeyPressedMessageHandler(HotkeyPressedMessage message)
        {
            Task.Run(async () =>
            {
                WriteLine($"Handling hotkey pressed message ({message.Hotkey}, {message.Action.ToString()})");

                if (_activeDevice.Device == null)
                {
                    string warning = TranslationSource.Instance["NoConnectedDevices"];
                    _messenger.Send(new ShowWarningNotificationMessage(warning));
                    WriteLine(warning, LogErrorSeverity.Warning);
                    return;
                }
                else if(!_activeDevice.Device.IsLoadingStorage)
                    await HandleHotkeyActionAsync(_activeDevice.Device.Id, message.Action, message.Hotkey);
            });
        }

        async Task HandleHotkeyActionAsync(string activeDeviceId, UserAction action, string hotkey)
        {
            try
            {
                await HandleActionAsync(activeDeviceId, action);
            }
            catch (HideezWindowSelectedException)
            {
                var localizedAction = $"press {hotkey}";
                var message = TranslationSource.Instance[$"UserAction.HideezWindowSelected.{action.ToString()}"];
                message = string.Format(message, Environment.NewLine, localizedAction);
                var msgOptions = new NotificationOptions { CloseTimeout = NotificationOptions.LongTimeout };
                _messenger.Send(new ShowInfoNotificationMessage(message, options: msgOptions, notificationId: _activeDevice.Device?.Mac));
            }
            catch (ActionNotSupportedException)
            {
                var localizedAction = TranslationSource.Instance[$"Enum.UserAction.{action.ToString()}"].ToLowerInvariant();
                _messenger.Send(new ShowInfoNotificationMessage($"Unable to {localizedAction}{Environment.NewLine}Action not supported", notificationId: _activeDevice.Device?.Mac));
            }
            catch (Exception ex)
            {
                _messenger.Send(new ShowErrorNotificationMessage(ex.Message, notificationId: _activeDevice.Device?.Mac));
                WriteLine(ex, LogErrorSeverity.Error);
            }
        }

        async Task HandleButtonActionAsync(string senderDeviceId, UserAction action, ButtonPressCode code)
        {
            try
            {
                await HandleActionAsync(senderDeviceId, action);
            }
            catch (HideezWindowSelectedException)
            {
                var localizedAction = $"press vault button {TranslationSource.Instance[$"Enum.ButtonPressCode.{code.ToString()}"]}";
                var message = TranslationSource.Instance[$"UserAction.HideezWindowSelected.{action.ToString()}"];
                message = string.Format(message, Environment.NewLine, localizedAction);
                var msgOptions = new NotificationOptions { CloseTimeout = NotificationOptions.LongTimeout };
                _messenger.Send(new ShowInfoNotificationMessage(message, options: msgOptions, notificationId: _activeDevice.Device?.Mac));
            }
            catch (ActionNotSupportedException)
            {
                var localizedAction = TranslationSource.Instance[$"Enum.UserAction.{action.ToString()}"].ToLowerInvariant();
                _messenger.Send(new ShowInfoNotificationMessage($"Unable to {localizedAction}{Environment.NewLine}Action not supported", notificationId: _activeDevice.Device?.Mac));
            }
            catch (Exception ex)
            {
                _messenger.Send(new ShowErrorNotificationMessage(ex.Message, notificationId: _activeDevice.Device?.Mac));
                WriteLine(ex, LogErrorSeverity.Error);
            }
        }

        async Task HandleActionAsync(string deviceId, UserAction action)
        {
            if (Interlocked.CompareExchange(ref _isPerformingActionLock, 1, 0) == 0)
            {
                WriteLine("User action lock engaged");
                try
                {
                    switch (action)
                    {
                        case UserAction.InputLogin:
                            await HandleInputActionAsync(deviceId, _inputLogin);
                            break;
                        case UserAction.InputPassword:
                            await HandleInputActionAsync(deviceId, _inputPassword);
                            break;
                        case UserAction.InputOtp:
                            await HandleInputActionAsync(deviceId, _inputOtp);
                            break;
                        case UserAction.AddAccount:
                            await OnAddNewAccount(deviceId);
                            break;
                        case UserAction.LockWorkstation:
                            _messenger.Send(new LockWorkstationMessage());
                            break;
                        default:
                            throw new NotImplementedException($"\"{action.ToString()}\" action is not implemented in UserActionHandler.");
                    }
                }
                finally
                {
                    await Task.Delay(DELAY_BEFORE_NEXT_ACTION);
                    Interlocked.Exchange(ref _isPerformingActionLock, 0);
                    WriteLine("User action lock lifted");
                }
            }
            else
                WriteLine($"{action.ToString()} canceled because another action is in progress");
        }

        async Task HandleInputActionAsync(string deviceId, IInputAlgorithm inputAlgorithm)
        {
            try
            {
                await inputAlgorithm.InputAsync(new[] { deviceId });
            }
            catch (OtpNotFoundException ex)
            {
                _messenger.Send(new ShowWarningNotificationMessage(ex.Message, notificationId: _activeDevice.Device?.Mac));
                WriteLine(ex.Message, LogErrorSeverity.Warning);
            }
            catch (AccountException ex) when (ex is LoginNotFoundException || ex is PasswordNotFoundException)
            {
                if (_appSettingsManager.Settings.UseSimplifiedUI)
                {
                    _messenger.Send(new ShowWarningNotificationMessage(ex.Message, notificationId: _activeDevice.Device?.Mac));
                    WriteLine(ex.Message, LogErrorSeverity.Warning);
                }
                else if (_appSettingsManager.Settings.AutoCreateAccountIfNotFound)
                {
                    await OnAddNewAccount(deviceId);
                }
                else
                {
                    var appInfo = AppInfoFactory.GetCurrentAppInfo();
                    WriteLine(ex.Message, LogErrorSeverity.Warning);
                    var createNewAccount = await _windowsManager.ShowAccountNotFoundAsync(ex.Message);

                    if (createNewAccount)
                        await OnAddNewAccount(deviceId, appInfo);
                }
            }
            catch (FieldNotSecureException) // Assume that precondition failed because field is not secure
            {
                string message = TranslationSource.Instance["Exception.FieldNotSecure"];
                _messenger.Send(new ShowWarningNotificationMessage(message, notificationId: _activeDevice.Device?.Mac));
                WriteLine(message, LogErrorSeverity.Warning);
            }
            catch (AuthEndedUnexpectedlyException)
            {
                var message = TranslationSource.Instance["Exception.AuthEndedUnexpectedly"];
                _messenger.Send(new ShowWarningNotificationMessage(message, notificationId: _activeDevice.Device?.Mac));
                WriteLine(message, LogErrorSeverity.Warning);
            }
        }

        async Task OnAddNewAccount(string deviceId, AppInfo appInfo = null)
        {
            if (_appSettingsManager.Settings.UseSimplifiedUI)
                throw new ActionNotSupportedException();

            if (appInfo == null)
                appInfo = AppInfoFactory.GetCurrentAppInfo();

            if (appInfo.ProcessName == "HideezClient")
                throw new HideezWindowSelectedException();

            if (_activeDevice.Device.IsLoadingStorage)
                return;

            if (!_activeDevice.Device.IsAuthorized || !_activeDevice.Device.IsStorageLoaded)
            {
                await _activeDevice.Device.InitRemoteAndLoadStorageAsync();
            }
            
            if (_activeDevice.Device.IsAuthorized && _activeDevice.Device.IsStorageLoaded)
            {
                _messenger.Send(new ShowInfoNotificationMessage($"Creating new account for {appInfo.Title}", notificationId: _activeDevice.Device?.Mac));
                _messenger.Send(new ShowActivateMainWindowMessage());
                _messenger.Send(new OpenPasswordManagerMessage(deviceId));
                _messenger.Send(new AddAccountForAppMessage(deviceId, appInfo));
            }
        }
    }
}
