using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Messages;
using HideezSafe.Modules.DeviceManager;
using System;
using Unity;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Modules.ActionHandler
{
    class UserActionHandler
    {
        private readonly IMessenger messenger;
        private readonly InputLogin inputLogin;
        private readonly InputPassword inputPassword;
        private readonly InputOtp inputOtp;

        public UserActionHandler(IMessenger messenger, InputLogin inputLogin, InputPassword inputPassword, InputOtp inputOtp)
        {
            this.messenger = messenger;
            this.inputLogin = inputLogin;
            this.inputPassword = inputPassword;
            this.inputOtp = inputOtp;

            this.messenger.Register<InputLoginMessage>(this, InputLoginMessageHandler);
            this.messenger.Register<InputPasswordMessage>(this, InputPasswordMessageHandler);
            this.messenger.Register<InputOtpMessage>(this, InputOtpMessageHandler);
        }

        private void InputLoginMessageHandler(InputLoginMessage message)
        {
            Task.Run(async () => await inputLogin.InputAsync(message.DevicesId));
        }

        private void InputPasswordMessageHandler(InputPasswordMessage message)
        {
            Task.Run(async () => await inputPassword.InputAsync(message.DevicesId));
        }

        private void InputOtpMessageHandler(InputOtpMessage message)
        {
            Task.Run(async () => await inputOtp.InputAsync(message.DevicesId));

        }
    }
}
