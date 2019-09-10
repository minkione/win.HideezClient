using NLog;
using HideezClient.Messages;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.HideezServiceReference;
using HideezClient.Messages.Remote;
using HideezClient.Extension;
using System;

namespace HideezClient.Modules.ServiceCallbackMessanger
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
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void DeviceAuthorized(DeviceDTO device)
        {
            try
            {
                log.Info($"Device ({device.Id}) is authorized");
                messenger.Send(new DeviceAuthorizedMessage(device));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void RemoteConnection_DeviceStateChanged(string deviceId, DeviceStateDTO stateDto)
        {
            try
            {
                //log.Info($"Remote ({deviceId}) system state received");
                messenger.Send(new Remote_DeviceStateChangedMessage(deviceId, stateDto.ToDeviceState()));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void ServiceComponentsStateChanged(bool hesConnected, bool showHesStatus, bool rfidConnected, bool showRfidStatus, bool bleConnected)
        {
            try
            {
                log.Info($"Service components state changed (hes:{hesConnected}; showHes:{showHesStatus}; " +
                    $"rfid:{rfidConnected}; showRfid:{showRfidStatus};  ble:{bleConnected})");
                messenger.Send(new ServiceComponentsStateChangedMessage(hesConnected, showHesStatus, rfidConnected, showRfidStatus, bleConnected));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void ServiceNotificationReceived(string message)
        {
            try
            {
                log.Info($"Notification message from service: {message}");
                messenger.Send(new ServiceNotificationReceivedMessage(message));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void ServiceErrorReceived(string error)
        {
            try
            {
                log.Info($"Error message from service: {error}");
                messenger.Send(new ServiceErrorReceivedMessage(error));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void ShowPinUi(string deviceId, bool withConfirm, bool askOldPin)
        {
            try
            {
                log.Info($"Show pin ui message for ({deviceId}; confirm: {withConfirm}; old pin: {askOldPin})");
                messenger.Send(new ShowPinUiMessage(deviceId, withConfirm, askOldPin));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void HidePinUi()
        {
            try
            {
                log.Info($"Hide pin ui message");
                messenger.Send(new HidePinUiMessage());
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
