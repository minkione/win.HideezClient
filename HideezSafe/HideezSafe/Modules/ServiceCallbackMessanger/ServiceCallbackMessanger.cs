using NLog;
using HideezSafe.Messages;
using GalaSoft.MvvmLight.Messaging;
using HideezSafe.HideezServiceReference;
using HideezSafe.Messages.Remote;
using System;

namespace HideezSafe.Modules.ServiceCallbackMessanger
{
    class ServiceCallbackMessanger : IHideezServiceCallback
    {
        private readonly IMessenger messenger;
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public ServiceCallbackMessanger(IMessenger messenger)
        {
            this.messenger = messenger;
        }

        public void ActivateWorkstationScreenRequest()
        {
            try
            {
                log.Info($"Activate screen request");
                messenger.Send(new ActivateScreenMessage());
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        public void LockWorkstationRequest()
        {
            try
            {
                log.Info($"Lock workstation request");
                messenger.Send(new LockWorkstationMessage());
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        public void DongleConnectionStateChanged(bool isConnected)
        {
            try
            {
                log.Info($"Dongle connection state changed: {isConnected}");
                messenger.Send(new ConnectionDongleChangedMessage(isConnected));
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        public void HESConnectionStateChanged(bool isConnected)
        {
            try
            {
                log.Info($"HES connection state changed: {isConnected}");
                messenger.Send(new ConnectionHESChangedMessage(isConnected));
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        public void RFIDConnectionStateChanged(bool isConnected)
        {
            try
            {
                log.Info($"RFID connection state changed: {isConnected}");
                messenger.Send(new ConnectionRFIDChangedMessage(isConnected));
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        public void DevicesCollectionChanged(DeviceDTO[] devices)
        {
            try
            {
                log.Info($"Paired devices collection changed. Count: {devices.Length}");
                messenger.Send(new DevicesCollectionChangedMessage(devices));
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        public void DeviceConnectionStateChanged(DeviceDTO device)
        {
            try
            {
                log.Info($"Device ({device.Id}) connection state changed to: {device.IsConnected}");
                messenger.Send(new DeviceConnectionStateChangedMessage(device));
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        public void DeviceInitialized(DeviceDTO device)
        {
            try
            {
                log.Info($"Device ({device.Id}) is initialized");
                messenger.Send(new DeviceInitializedMessage(device));
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        public void RemoteConnection_RssiReceived(string serialNo, double rssi)
        {
            try
            {
                // to many messages are printed into log due to the line bellow
                //log.Info($"Remote ({connectionId}) rssi received ({rssi})");
                messenger.Send(new Remote_RssiReceivedMessage(serialNo, rssi));
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        public void RemoteConnection_BatteryChanged(string serialNo, int battery)
        {
            try
            {
                log.Info($"Remote ({serialNo}) battery changed to {battery}");
                messenger.Send(new Remote_BatteryChangedMessage(serialNo, battery));
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        public void RemoteConnection_StorageModified(string serialNo)
        {
            try
            {
                log.Info($"Remote ({serialNo}) storage modified");
                messenger.Send(new Remote_StorageModifiedMessage(serialNo));
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
