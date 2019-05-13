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

        public void DongleConnectionStateChanged(bool isConnected)
        {
            messenger.Send(new ConnectionDongleChangedMessage(isConnected));
        }

        public void HESConnectionStateChanged(bool isConnected)
        {
            messenger.Send(new ConnectionHESChangedMessage(isConnected));
        }

        public void RFIDConnectionStateChanged(bool isConnected)
        {
            messenger.Send(new ConnectionRFIDChangedMessage(isConnected));
        }

        public void LockWorkstationRequest()
        {
            messenger.Send(new LockWorkstationMessage());
        }

        public void PairedDevicesCollectionChanged(BleDeviceDTO[] devices)
        {
            messenger.Send(new PairedDevicesCollectionChangedMessage(devices));
        }

        public void PairedDevicePropertyChanged(BleDeviceDTO device)
        {
            messenger.Send(new DevicePropertiesUpdatedMessage(device));
        }

    }
}
