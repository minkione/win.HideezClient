using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Messages;
using HideezSafe.Modules.DeviceManager;
using System;
using Unity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HideezSafe.Modules.SettingsManager;
using HideezSafe.Models.Settings;

namespace HideezSafe.Modules.ActionHandler
{
    class UserActionHandler
    {
        private readonly IWindowsManager windowsManager;
        private readonly IMessenger messenger;
        protected readonly ISettingsManager<ApplicationSettings> settingsManager;
        private readonly InputLogin inputLogin;
        private readonly InputPassword inputPassword;
        private readonly InputOtp inputOtp;

        public UserActionHandler(IWindowsManager windowsManager, IMessenger messenger, ISettingsManager<ApplicationSettings> settingsManager, InputLogin inputLogin, InputPassword inputPassword, InputOtp inputOtp)
        {
            this.windowsManager = windowsManager;
            this.messenger = messenger;
            this.settingsManager = settingsManager;
            this.inputLogin = inputLogin;
            this.inputPassword = inputPassword;
            this.inputOtp = inputOtp;

            this.messenger.Register<InputLoginMessage>(this, InputLoginMessageHandler);
            this.messenger.Register<InputPasswordMessage>(this, InputPasswordMessageHandler);
            this.messenger.Register<InputOtpMessage>(this, InputOtpMessageHandler);
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
                windowsManager.ShowError(ex.Message);
                // TODO: implement logic if not found data for account
            }
            catch (FieldNotSecureException) // Assume that precondition failed because field is not secure
            {
                windowsManager.ShowError(Localize.TranslationSource.Instance["Exception.FieldNotSecure"]);
            }
            catch (Exception ex)
            {
                windowsManager.ShowError(ex.Message);
            }
        }
    }
}
