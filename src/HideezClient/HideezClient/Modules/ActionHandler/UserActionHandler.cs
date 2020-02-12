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

namespace HideezClient.Modules.ActionHandler
{
    /// <summary>
    /// Responsible for receiving user action requests and routing them for execution
    /// </summary>
    class UserActionHandler : Logger
    {
        private readonly IMessenger _messenger;
        private readonly IActiveDevice _activeDevice;
        private readonly InputLogin inputLogin;
        private readonly InputPassword inputPassword;
        private readonly InputOtp inputOtp;

        public UserActionHandler(
            IMessenger messenger,
            IActiveDevice activeDevice,
            InputLogin inputLogin,
            InputPassword inputPassword,
            InputOtp inputOtp,
            ILog log)
            : base(nameof(UserActionHandler), log)
        {
            _messenger = messenger;
            _activeDevice = activeDevice;
            this.inputLogin = inputLogin;
            this.inputPassword = inputPassword;
            this.inputOtp = inputOtp;

            _messenger.Register<HotkeyPressedMessage>(this, HotkeyPressedMessageHandler);
            _messenger.Register<ButtonPressedMessage>(this, ButtonPressedMessageHandler);
        }

        private IInputAlgorithm GetInputAlgorithm(UserAction userAction)
        {
            IInputAlgorithm input;
            switch (userAction)
            {
                case UserAction.InputLogin:
                    input = inputLogin;
                    break;
                case UserAction.InputPassword:
                    input = inputPassword;
                    break;
                case UserAction.InputOtp:
                    input = inputOtp;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return input;
        }

        private void ButtonPressedMessageHandler(ButtonPressedMessage message)
        {
            WriteLine("Handling button pressed message.");

            if (_activeDevice.Device?.Id == message.DeviceId)
                Task.Run(async () => await InputAccountAsync(new[] { message.DeviceId }, GetInputAlgorithm(message.Action)));
            else
            {
                string warn = string.Format(TranslationSource.Instance["DeviceIsNotActive"], message.DeviceId);
                _messenger.Send(new ShowWarningNotificationMessage(warn));
                WriteLine(warn, LogErrorSeverity.Warning);
            }
        }

        private void HotkeyPressedMessageHandler(HotkeyPressedMessage hotkeyMessage)
        {
            WriteLine("Handling hotkey pressed message.");

            if (_activeDevice.Device == null)
            {
                string message = TranslationSource.Instance["NoAnyConnectedDevice"];
                _messenger.Send(new ShowWarningNotificationMessage(message));
                WriteLine(message, LogErrorSeverity.Warning);
                return;
            }
            else
                Task.Run(async () => await InputAccountAsync(new[] { _activeDevice.Device.Id }, GetInputAlgorithm(hotkeyMessage.Action)));

        }

        private async Task InputAccountAsync(string[] devicesId, IInputAlgorithm inputAlgorithm)
        {
            if (inputAlgorithm == null)
            {
                string message = $" ArgumentNull: {nameof(inputAlgorithm)}.";
                _messenger.Send(new ShowErrorNotificationMessage(message));
                WriteLine(message, LogErrorSeverity.Error);
                return;
            }

            try
            {
                await inputAlgorithm.InputAsync(devicesId);
            }
            catch (HideezWindowSelectedException ex)
            {
                var msgOptions = new NotificationOptions { CloseTimeout = TimeSpan.FromSeconds(30) };
                _messenger.Send(new ShowInfoNotificationMessage(ex.Message, options: msgOptions));
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
            catch (Exception ex)
            {
                _messenger.Send(new ShowErrorNotificationMessage(ex.Message));
                WriteLine(ex, LogErrorSeverity.Error);
            }
        }
    }
}
