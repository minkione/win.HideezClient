using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;

namespace HideezClient.Modules
{
    class PinHelper : IPinHelper
    {
        readonly IMessenger _messenger;
        readonly IWindowsManager _windowManager;

        public PinHelper(IMessenger messenger, IWindowsManager windowManager)
        {
            _messenger = messenger;
            _windowManager = windowManager;

            _messenger.Register<ShowPinUiMessage>(this, OnShowPinUi);
            _messenger.Register<HidePinUiMessage>(this, OnHidePinUi);
        }

        void OnShowPinUi(ShowPinUiMessage message)
        {
            // Todo: Show pin UI
        }

        void OnHidePinUi(HidePinUiMessage message)
        {
            // Todo: Hide pin UI
        }
    }
}
