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
        readonly IMessenger _messenger;
        readonly Logger log = LogManager.GetCurrentClassLogger();

        public ServiceCallbackMessanger(IMessenger messenger)
        {
            this._messenger = messenger;
        }

        public void ActivateWorkstationScreenRequest()
        {
            try
            {
                log.Info($"Activate screen request");
                _messenger.Send(new ActivateScreenMessage());
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
                _messenger.Send(new LockWorkstationMessage());
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
                _messenger.Send(new DevicesCollectionChangedMessage(devices));
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
                _messenger.Send(new DeviceConnectionStateChangedMessage(device));
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
                _messenger.Send(new DeviceInitializedMessage(device));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void DeviceFinishedMainFlow(DeviceDTO device)
        {
            try
            {
                log.Info($"Device ({device.Id}) has finished main flow");
                _messenger.Send(new DeviceFinishedMainFlowMessage(device));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void DeviceOperationCancelled(DeviceDTO device)
        {
            try
            {
                log.Info($"Device ({device.Id}) operation cancelled");
                _messenger.Send(new DeviceOperationCancelledMessage(device));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void DeviceProximityChanged(string deviceId, double proximity)
        {
            try
            {
                log.Info($"Device ({deviceId}) proximity changed to {proximity}");
                _messenger.Send(new DeviceProximityChangedMessage(deviceId, proximity));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void DeviceBatteryChanged(string deviceId, int battery)
        {
            try
            {
                log.Info($"Device ({deviceId}) battery changed to {battery}");
                _messenger.Send(new DeviceBatteryChangedMessage(deviceId, battery));
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
                _messenger.Send(new Remote_DeviceStateChangedMessage(deviceId, stateDto.ToDeviceState()));
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
                _messenger.Send(new ServiceComponentsStateChangedMessage(hesConnected, showHesStatus, rfidConnected, showRfidStatus, bleConnected));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void ServiceNotificationReceived(string message, string notificationId)
        {
            try
            {
                log.Info($"Notification message from service: {message} ({notificationId})");
                _messenger.Send(new ServiceNotificationReceivedMessage(notificationId, message));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void ServiceErrorReceived(string error, string notificationId)
        {
            try
            {
                log.Info($"Error message from service: {error} ({notificationId})");
                _messenger.Send(new ServiceErrorReceivedMessage(notificationId, error));
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
                _messenger.Send(new ShowPinUiMessage(deviceId, withConfirm, askOldPin));
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        public void ShowButtonConfirmUi(string deviceId)
        {
            try
            {
                log.Info($"Show button ui message for ({deviceId})");
                _messenger.Send(new ShowButtonConfirmUiMessage(deviceId));
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
                _messenger.Send(new HidePinUiMessage());
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
