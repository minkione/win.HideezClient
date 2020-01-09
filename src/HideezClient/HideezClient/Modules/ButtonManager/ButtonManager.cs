using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication;
using HideezClient.Messages;
using HideezClient.Models;
using System.Threading.Tasks;

namespace HideezClient.Modules.ButtonManager
{
    internal sealed class ButtonManager : IButtonManager
    {
        readonly IMessenger _messenger;
        bool _enabled;

        public ButtonManager(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnManagerEnabledChanged(value);
                }
            }
        }

        void OnManagerEnabledChanged(bool newValue)
        {
            Task.Run(() =>
            {
                if (newValue)
                {
                    _messenger.Register<DeviceButtonPressedMessage>(this, OnDeviceButtonPressed);
                }
                else
                {
                    _messenger.Unregister<DeviceButtonPressedMessage>(this);
                }
            });
        }

        void OnDeviceButtonPressed(DeviceButtonPressedMessage msg)
        {
            UserAction action;
            switch (msg.Button)
            {
                case ButtonPressCode.Single:
                    action = UserAction.InputLogin;
                    break;
                case ButtonPressCode.Double:
                    action = UserAction.InputPassword;
                    break;
                case ButtonPressCode.Triple:
                    action = UserAction.InputOtp;
                    break;
                default:
                    return;
            }

            _messenger.Send(new ButtonPressedMessage(msg.DeviceId, action));
        }
    }
}
