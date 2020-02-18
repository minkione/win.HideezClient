using System;
using System.Linq;
using HideezClient.Messages;
using System.Threading.Tasks;
using HideezClient.Modules.Localize;
using HideezClient.Modules.DeviceManager;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.Models;
using System.Diagnostics;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication;
using System.Threading;
using Hideez.ARM;
using HideezClient.Models.Settings;
using HideezMiddleware.Settings;

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

        public UserActionHandler(
            IMessenger messenger,
            IActiveDevice activeDevice,
            InputLogin inputLogin,
            InputPassword inputPassword,
            InputOtp inputOtp,
            ISettingsManager<ApplicationSettings> appSettingsManager,
            ILog log)
            : base(nameof(UserActionHandler), log)
        {
            _messenger = messenger;
            _activeDevice = activeDevice;
            _inputLogin = inputLogin;
            _inputPassword = inputPassword;
            _inputOtp = inputOtp;
            _appSettingsManager = appSettingsManager;

            _messenger.Register<HotkeyPressedMessage>(this, HotkeyPressedMessageHandler);
            _messenger.Register<ButtonPressedMessage>(this, ButtonPressedMessageHandler);
        }

        void ButtonPressedMessageHandler(ButtonPressedMessage message)
        {
            WriteLine($"Handling button pressed message ({message.Action.ToString()}, {message.Code.ToString()})");

            if (_activeDevice.Device?.Id == message.DeviceId)
                Task.Run(async () => await HandleButtonActionAsync(message.DeviceId, message.Action, message.Code));
            else
            {
                string warn = string.Format(TranslationSource.Instance["DeviceIsNotActive"], message.DeviceId);
                _messenger.Send(new ShowWarningNotificationMessage(warn));
                WriteLine(warn, LogErrorSeverity.Warning);
            }
        }

        void HotkeyPressedMessageHandler(HotkeyPressedMessage message)
        {
            WriteLine($"Handling hotkey pressed message ({message.Hotkey}, {message.Action.ToString()})");

            if (_activeDevice.Device == null)
            {
                string warning = TranslationSource.Instance["NoAnyConnectedDevice"];
                _messenger.Send(new ShowWarningNotificationMessage(warning));
                WriteLine(warning, LogErrorSeverity.Warning);
                return;
            }
            else
                Task.Run(async () => await HandleHotkeyActionAsync(_activeDevice.Device.Id, message.Action, message.Hotkey));

        }

        async Task HandleHotkeyActionAsync(string activeDeviceId, UserAction action, string hotkey)
        {
            try
            {
                await HandleActionAsync(activeDeviceId, action);
            }
            catch (HideezWindowSelectedException ex)
            {
                // TODO: Format message according to action and hotkey
                var msgOptions = new NotificationOptions { CloseTimeout = TimeSpan.FromSeconds(30) };
                _messenger.Send(new ShowInfoNotificationMessage(ex.Message, options: msgOptions));
            }
            catch (ActionNotSupportedException ex)
            {
                // TODO: Format message according to action and hotkey
                _messenger.Send(new ShowInfoNotificationMessage(ex.Message));
            }
            catch (Exception ex)
            {
                _messenger.Send(new ShowErrorNotificationMessage(ex.Message));
                WriteLine(ex, LogErrorSeverity.Error);
            }
        }

        async Task HandleButtonActionAsync(string senderDeviceId, UserAction action, ButtonPressCode code)
        {
            try
            {
                await HandleActionAsync(senderDeviceId, action);
            }
            catch (HideezWindowSelectedException ex)
            {
                // TODO: Format message according to action and button code
                var msgOptions = new NotificationOptions { CloseTimeout = TimeSpan.FromSeconds(30) };
                _messenger.Send(new ShowInfoNotificationMessage(ex.Message, options: msgOptions));
            }
            catch (ActionNotSupportedException ex)
            {
                // TODO: Format message according to action and button code
                _messenger.Send(new ShowInfoNotificationMessage(ex.Message));
            }
            catch (Exception ex)
            {
                _messenger.Send(new ShowErrorNotificationMessage(ex.Message));
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
                        case UserAction.AddPassword:
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
                    await Task.Delay(500);
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
                _messenger.Send(new ShowWarningNotificationMessage(ex.Message));
                WriteLine(ex.Message, LogErrorSeverity.Warning);
            }
            catch (AccountException ex) when (ex is LoginNotFoundException || ex is PasswordNotFoundException)
            {
                _messenger.Send(new ShowWarningNotificationMessage(ex.Message));
                WriteLine(ex.Message, LogErrorSeverity.Warning);
            }
            catch (FieldNotSecureException) // Assume that precondition failed because field is not secure
            {
                string message = TranslationSource.Instance["Exception.FieldNotSecure"];
                _messenger.Send(new ShowWarningNotificationMessage(message));
                WriteLine(message, LogErrorSeverity.Warning);
            }
        }

        async Task OnAddNewAccount(string deviceId)
        {
            if (_appSettingsManager.Settings.UseSimplifiedUI)
                throw new ActionNotSupportedException();

            var appInfo = AppInfoFactory.GetCurrentAppInfo();

            if (appInfo.ProcessName == "HideezClient")
                throw new HideezWindowSelectedException();

            if (!_activeDevice.Device.IsAuthorized || !_activeDevice.Device.IsStorageLoaded)
            {
                await _activeDevice.Device.InitRemoteAndLoadStorageAsync();
            }
            
            if (_activeDevice.Device.IsAuthorized && _activeDevice.Device.IsStorageLoaded)
            {
                _messenger.Send(new ShowActivateMainWindowMessage());
                _messenger.Send(new OpenPasswordManagerMessage(deviceId));
                _messenger.Send(new AddAccountForAppMessage(deviceId, appInfo));
            }
        }
    }
}
