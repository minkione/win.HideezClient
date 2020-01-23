using NLog;
using HideezClient.Messages;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.HideezServiceReference;
using HideezClient.Messages.Remote;
using HideezClient.Extension;
using System;
using System.Threading.Tasks;

namespace HideezClient.Modules.ServiceCallbackMessanger
{
    class ServiceCallbackMessanger : IHideezServiceCallback
    {
        readonly IMessenger _messenger;
        readonly Logger log = LogManager.GetCurrentClassLogger();

        public ServiceCallbackMessanger(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public void ActivateWorkstationScreenRequest()
        {
            log.Info($"Activate screen request");
            SendMessage(new ActivateScreenMessage());
        }

        public void LockWorkstationRequest()
        {
            log.Info($"Lock workstation request");
            SendMessage(new LockWorkstationMessage());
        }

        public void DevicesCollectionChanged(DeviceDTO[] devices)
        {
            log.Info($"Paired devices collection changed. Count: {devices.Length}");
            SendMessage(new DevicesCollectionChangedMessage(devices));
        }

        public void DeviceConnectionStateChanged(DeviceDTO device)
        {
            log.Info($"Device ({device.Id}) connection state changed to: {device.IsConnected}");
            SendMessage(new DeviceConnectionStateChangedMessage(device));
        }

        public void DeviceInitialized(DeviceDTO device)
        {
            log.Info($"Device ({device.Id}) is initialized");
            SendMessage(new DeviceInitializedMessage(device));
        }

        public void DeviceFinishedMainFlow(DeviceDTO device)
        {
            log.Info($"Device ({device.Id}) has finished main flow");
            SendMessage(new DeviceFinishedMainFlowMessage(device));
        }

        public void DeviceOperationCancelled(DeviceDTO device)
        {
            log.Info($"Device ({device.Id}) operation cancelled");
            SendMessage(new DeviceOperationCancelledMessage(device));
        }

        public void DeviceProximityChanged(string deviceId, double proximity)
        {
            log.Info($"Device ({deviceId}) proximity changed to {proximity}");
            SendMessage(new DeviceProximityChangedMessage(deviceId, proximity));
        }

        public void DeviceBatteryChanged(string deviceId, int battery)
        {
            log.Info($"Device ({deviceId}) battery changed to {battery}");
            SendMessage(new DeviceBatteryChangedMessage(deviceId, battery));
        }

        public void RemoteConnection_DeviceStateChanged(string deviceId, DeviceStateDTO stateDto)
        {
            //log.Info($"Remote ({deviceId}) system state received");
            SendMessage(new Remote_DeviceStateChangedMessage(deviceId, stateDto.ToDeviceState()));
        }

        public void ServiceComponentsStateChanged(bool hesConnected, bool showHesStatus, bool rfidConnected, bool showRfidStatus, bool bleConnected)
        {
            log.Info($"Service components state changed (hes:{hesConnected}; showHes:{showHesStatus}; " +
                $"rfid:{rfidConnected}; showRfid:{showRfidStatus};  ble:{bleConnected})");
            SendMessage(new ServiceComponentsStateChangedMessage(hesConnected, showHesStatus, rfidConnected, showRfidStatus, bleConnected));
        }

        public void ServiceNotificationReceived(string message, string notificationId)
        {
            log.Info($"Notification message from service: {message} ({notificationId})");
            SendMessage(new ServiceNotificationReceivedMessage(notificationId, message));
        }

        public void ServiceErrorReceived(string error, string notificationId)
        {
            log.Info($"Error message from service: {error} ({notificationId})");
            SendMessage(new ServiceErrorReceivedMessage(notificationId, error));
        }

        public void ShowPinUi(string deviceId, bool withConfirm, bool askOldPin)
        {
            log.Info($"Show pin ui message for ({deviceId}; confirm: {withConfirm}; old pin: {askOldPin})");
            SendMessage(new ShowPinUiMessage(deviceId, withConfirm, askOldPin));
        }

        public void ShowButtonConfirmUi(string deviceId)
        {
            log.Info($"Show button ui message for ({deviceId})");
            SendMessage(new ShowButtonConfirmUiMessage(deviceId));
        }

        public void HidePinUi()
        {
            log.Info($"Hide pin ui message");
            SendMessage(new HidePinUiMessage());
        }

        public void DeviceProximityLockEnabled(DeviceDTO device)
        {
            log.Info($"Device ({device.Id}) marked as valid for workstation lock");
            SendMessage(new DeviceProximityLockEnabledMessage(device));
        }

        /// <summary>
        /// Send message without blocking current thread using IMessenger
        /// </summary>
        /// <param name="message">Message to send using IMessenger</param>
        void SendMessage<T>(T message)
        {
            Task.Run(() =>
            {
                try
                { 
                    _messenger.Send(message);
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }
            });
        }

        public void ProximitySettingsChanged()
        {
            try
            {
                log.Info($"Device proximity settings changed");
                    _messenger.Send(new DeviceProximitySettingsChangedMessage());
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
