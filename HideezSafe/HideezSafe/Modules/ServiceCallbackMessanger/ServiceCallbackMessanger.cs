using GalaSoft.MvvmLight.Messaging;
using HideezSafe.HideezServiceReference;
using HideezSafe.Messages;

namespace HideezSafe.Modules.ServiceCallbackMessanger
{
    class ServiceCallbackMessanger : IHideezServiceCallback
    {
        // Todo: Implement callback events through MvvmLight messanger 
        private readonly IMessenger messenger;

        public ServiceCallbackMessanger(IMessenger messenger)
        {
            this.messenger = messenger;
        }

        public void ConnectionDongleChangedRequest(bool isConnected)
        {
            messenger.Send(new ConnectionDongleChangedMessage(isConnected));
        }

        public void ConnectionHESChangedRequest(bool isConnected)
        {
            messenger.Send(new ConnectionHESChangedMessage(isConnected));
        }

        public void ConnectionRFIDChangedRequest(bool isConnected)
        {
            messenger.Send(new ConnectionRFIDChangedMessage(isConnected));
        }

        public void LockWorkstationRequest()
        {
            messenger.Send(new LockWorkstationMessage());
        }
    }
}
