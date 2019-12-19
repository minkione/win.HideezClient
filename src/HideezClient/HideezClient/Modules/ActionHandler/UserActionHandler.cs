using System;
using System.Linq;
using HideezClient.Messages;
using System.Threading.Tasks;
using HideezClient.Modules.Localize;
using HideezClient.Modules.DeviceManager;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.Models;
using NLog;
using System.Diagnostics;

namespace HideezClient.Modules.ActionHandler
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
            log.Info("Handling button pressed message.");
            Task.Run(async () => await InputAccountAsync(new[] { message.DeviceId }, GetInputAlgorithm(message.Action)));
        }

        private void HotkeyPressedMessageHandler(HotkeyPressedMessage hotkeyMessage)
        {
            log.Info("Handling hotkey pressed message.");
            string[] devicesId = deviceManager.Devices.Where(d => d.IsConnected).Select(d => d.Id).ToArray();

            if (!devicesId.Any())
            {
                string message = TranslationSource.Instance["NoAnyConnectedDevice"];
                windowsManager.ShowWarn(message);
                log.Warn(message);
                return;
            }

            Task.Run(async () => await InputAccountAsync(devicesId, GetInputAlgorithm(hotkeyMessage.Action)));

        }

        private async Task InputAccountAsync(string[] devicesId, IInputAlgorithm inputAlgorithm)
        {
            if (inputAlgorithm == null)
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
            catch (OtpNotFoundException ex)
            {
                windowsManager.ShowWarn(ex.Message);
                log.Warn(ex.Message);
            }
            catch (AccountException ex) when (ex is LoginNotFoundException || ex is PasswordNotFoundException)
            {
                string message = string.Format(TranslationSource.Instance["Exception.AccountNotFound"], ex.AppInfo.Title);
                windowsManager.ShowWarn(message);
                log.Warn(message);
            }
            catch (FieldNotSecureException) // Assume that precondition failed because field is not secure
            {
                string message = TranslationSource.Instance["Exception.FieldNotSecure"];
                windowsManager.ShowWarn(message);
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
