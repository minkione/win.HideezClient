using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Messages;
using System;
using System.Threading.Tasks;
using HideezSafe.Modules.Localize;

namespace HideezSafe.Modules.ActionHandler
{
    /// <summary>
    /// Responsible for receiving user action requests and routing them for execution
    /// </summary>
    class UserActionHandler
    {
        readonly IWindowsManager windowsManager;
        readonly InputLogin inputLogin;
        readonly InputPassword inputPassword;
        readonly InputOtp inputOtp;

        public UserActionHandler(
            IWindowsManager windowsManager, 
            IMessenger messenger, 
            InputLogin inputLogin, 
            InputPassword inputPassword, 
            InputOtp inputOtp)
        {
            this.windowsManager = windowsManager;

            this.inputLogin = inputLogin;
            this.inputPassword = inputPassword;
            this.inputOtp = inputOtp;

            messenger.Register<InputLoginMessage>(this, InputLoginMessageHandler);
            messenger.Register<InputPasswordMessage>(this, InputPasswordMessageHandler);
            messenger.Register<InputOtpMessage>(this, InputOtpMessageHandler);
        }

        private void InputLoginMessageHandler(InputLoginMessage message)
        {
            Task.Run(async () => await InputAccountAsync(message.DevicesId, inputLogin));
        }

        private void InputPasswordMessageHandler(InputPasswordMessage message)
        {
            Task.Run(async () => await InputAccountAsync(message.DevicesId, inputPassword));
        }

        private void InputOtpMessageHandler(InputOtpMessage message)
        {
            Task.Run(async () => await InputAccountAsync(message.DevicesId, inputOtp));

        }

        private async Task InputAccountAsync(string[] devicesId, IInputAlgorithm inputAlgorithm)
        {
            try
            {
                await inputAlgorithm.InputAsync(devicesId);
            }
            catch (AccountException ex) when (ex is LoginNotFoundException || ex is PasswordNotFoundException || ex is OtpNotFoundException)
            {
                windowsManager.ShowError(TranslationSource.Instance["AppName"], string.Format(TranslationSource.Instance["Exception.AccountNotFound"], ex.AppInfo.Title));
            }
            catch (FieldNotSecureException) // Assume that precondition failed because field is not secure
            {
                windowsManager.ShowError(TranslationSource.Instance["AppName"], TranslationSource.Instance["Exception.FieldNotSecure"]);
            }
            catch (Exception ex)
            {
                windowsManager.ShowError(ex.Message);
            }
        }
    }
}
