using System;
using System.Linq;
using HideezSafe.Messages;
using System.Threading.Tasks;
using HideezSafe.Modules.Localize;
using HideezSafe.Modules.DeviceManager;
using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Models;
using NLog;
using System.Diagnostics;

namespace HideezSafe.Modules.ActionHandler
{
    /// <summary>
    /// Responsible for receiving user action requests and routing them for execution
    /// </summary>
    class UserActionHandler
    {
        readonly ILogger log = LogManager.GetCurrentClassLogger();
        private readonly IWindowsManager windowsManager;
        private readonly IDeviceManager deviceManager;
        private readonly InputLogin inputLogin;
        private readonly InputPassword inputPassword;
        private readonly InputOtp inputOtp;

        public UserActionHandler(
            IWindowsManager windowsManager,
            IMessenger messenger,
            IDeviceManager deviceManager,
            InputLogin inputLogin,
            InputPassword inputPassword,
            InputOtp inputOtp)
        {
            this.windowsManager = windowsManager;
            this.deviceManager = deviceManager;
            this.inputLogin = inputLogin;
            this.inputPassword = inputPassword;
            this.inputOtp = inputOtp;

            messenger.Register<HotkeyPressedMessage>(this, HotkeyPressedMessageHandler);
            messenger.Register<ButtonPressedMessage>(this, ButtonPressedMessageHandler);
        }

        private IInputAlgorithm GetInputAlgorithm(UserAction userAction)
        {
            IInputAlgorithm input = null;
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
            log.Info("Handling button pressed message.");
            Task.Run(async () => await InputAccountAsync(new[] { message.DeviceId }, GetInputAlgorithm(message.Action)));
        }

        private void HotkeyPressedMessageHandler(HotkeyPressedMessage message)
        {
            log.Info("Handling hotkey pressed message.");
            string[] devicesId = deviceManager.Devices.Where(d => d.IsConnected).Select(d => d.Id).ToArray();

            Task.Run(async () => await InputAccountAsync(devicesId, GetInputAlgorithm(message.Action)));

        }

        private async Task InputAccountAsync(string[] devicesId, IInputAlgorithm inputAlgorithm)
        {
            if (inputAlgorithm != null)
            {
                string message = $" ArgumentNull: {nameof(inputAlgorithm)}.";
                log.Error(message);
                Debug.Assert(false, message);
                return;
            }

            try
            {
                await inputAlgorithm.InputAsync(devicesId);
            }
            catch (AccountException ex) when (ex is LoginNotFoundException || ex is PasswordNotFoundException || ex is OtpNotFoundException)
            {
                string message = string.Format(TranslationSource.Instance["Exception.AccountNotFound"], ex.AppInfo.Title);
                windowsManager.ShowError(message);
                log.Error(message);
            }
            catch (FieldNotSecureException) // Assume that precondition failed because field is not secure
            {
                string message = TranslationSource.Instance["Exception.FieldNotSecure"];
                windowsManager.ShowWarning(message);
                log.Warn(message);
            }
            catch (Exception ex)
            {
                windowsManager.ShowError(ex.Message);
                log.Error(ex);
            }
        }
    }
}
