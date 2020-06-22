using HideezClient.Messages;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.HideezServiceReference;
using HideezClient.Messages.Remote;
using HideezClient.Extension;
using System;
using System.Threading.Tasks;
using Hideez.SDK.Communication.Log;
using HideezClient.Modules.Log;
using HideezMiddleware.Threading;

namespace HideezClient.Modules.ServiceCallbackMessanger
{
    class ServiceCallbackMessanger : IHideezServiceCallback
    {
        readonly IMessenger _messenger;
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(ServiceCallbackMessanger));
        readonly SemaphoreQueue _sendMessageSemaphore = new SemaphoreQueue(1, 1);

        public ServiceCallbackMessanger(IMessenger messenger)
        {
            _messenger = messenger;
        }

        public void ActivateWorkstationScreenRequest()
        {
            _log.WriteLine($"Activate screen request");
            SendMessage(new ActivateScreenMessage());
        }

        public void LockWorkstationRequest()
        {
            _log.WriteLine($"Lock workstation request");
            SendMessage(new LockWorkstationMessage());
        }

        public void DevicesCollectionChanged(DeviceDTO[] devices)
        {
            _log.WriteLine($"Paired devices collection changed. Count: {devices.Length}");
            SendMessage(new DevicesCollectionChangedMessage(devices));
        }

        public void DeviceConnectionStateChanged(DeviceDTO device)
        {
            _log.WriteLine($"({device.Id}) Device connection state changed to: {device.IsConnected}");
            SendMessage(new DeviceConnectionStateChangedMessage(device));
        }

        public void DeviceInitialized(DeviceDTO device)
        {
            _log.WriteLine($"({device.Id}) Device is initialized");
            SendMessage(new DeviceInitializedMessage(device));
        }

        public void DeviceFinishedMainFlow(DeviceDTO device)
        {
            _log.WriteLine($"({device.Id}) Device has finished main flow");
            SendMessage(new DeviceFinishedMainFlowMessage(device));
        }

        public void DeviceOperationCancelled(DeviceDTO device)
        {
            _log.WriteLine($"({device.Id}) Device operation cancelled");
            SendMessage(new DeviceOperationCancelledMessage(device));
        }

        public void DeviceProximityChanged(string deviceId, double proximity)
        {
            _log.WriteLine($"({deviceId}) Device proximity changed to {proximity}");
            SendMessage(new DeviceProximityChangedMessage(deviceId, proximity));
        }

        public void DeviceBatteryChanged(string deviceId, int battery)
        {
            _log.WriteLine($"({deviceId}) Device battery changed to {battery}");
            SendMessage(new DeviceBatteryChangedMessage(deviceId, battery));
        }

        public void RemoteConnection_DeviceStateChanged(string deviceId, DeviceStateDTO stateDto)
        {
            //_log.WriteLine($"({deviceId}) Remote system state received");
            SendMessage(new Remote_DeviceStateChangedMessage(deviceId, stateDto.ToDeviceState()));
        }

        public void ServiceComponentsStateChanged(HesStatus hesStatus, RfidStatus rfidStatus, BluetoothStatus bluetoothStatus, HesStatus tbHesStatus)
        {
            _log.WriteLine($"Service components state changed (hes:{hesStatus}; rfid:{rfidStatus}; ble:{bluetoothStatus}; tbHes:{tbHesStatus};)");
            SendMessage(new ServiceComponentsStateChangedMessage(hesStatus, rfidStatus, bluetoothStatus, tbHesStatus));
        }

        public void ServiceNotificationReceived(string message, string notificationId)
        {
            _log.WriteLine($"Notification message from service: {message} ({notificationId})");
            SendMessage(new ServiceNotificationReceivedMessage(notificationId, message));
        }

        public void ServiceErrorReceived(string error, string notificationId)
        {
            _log.WriteLine($"Error message from service: {error} ({notificationId})");
            SendMessage(new ServiceErrorReceivedMessage(notificationId, error));
        }

        public void ShowPinUi(string deviceId, bool withConfirm, bool askOldPin)
        {
            _log.WriteLine($"Show pin ui message for ({deviceId}; confirm: {withConfirm}; old pin: {askOldPin})");
            SendMessage(new ShowPinUiMessage(deviceId, withConfirm, askOldPin));
        }

        public void ShowButtonConfirmUi(string deviceId)
        {
            _log.WriteLine($"Show button ui message for ({deviceId})");
            SendMessage(new ShowButtonConfirmUiMessage(deviceId));
        }

        public void HidePinUi()
        {
            _log.WriteLine($"Hide pin ui message");
            SendMessage(new HidePinUiMessage());
        }

        public void ShowActivationCodeUi(string deviceId)
        {
            _log.WriteLine($"Show activation code ui message for ({deviceId})");
            SendMessage(new ShowActivationCodeUiMessage(deviceId));
        }

        public void HideActivationCodeUi()
        {
            _log.WriteLine($"Hide activation code ui message");
            SendMessage(new HideActivationCodeUiMessage());
        }

        public void DeviceProximityLockEnabled(DeviceDTO device)
        {
            _log.WriteLine($"({device.Id}) Device marked as valid for workstation lock");
            SendMessage(new DeviceProximityLockEnabledMessage(device));
        }

        /// <summary>
        /// Send message without blocking current thread using IMessenger
        /// </summary>
        /// <param name="message">Message to send using IMessenger</param>
        void SendMessage<T>(T message)
        {
            Task.Run(async () =>
            {
                await _sendMessageSemaphore.WaitAsync();
                try
                { 
                    _messenger.Send(message);
                }
                catch (Exception ex)
                {
                    _log.WriteLine(ex);
                }
                finally
                {
                    _sendMessageSemaphore.Release();
                }
            });
        }

        public void ProximitySettingsChanged()
        {
            try
            {
                _log.WriteLine($"Device proximity settings changed");
                _messenger.Send(new DeviceProximitySettingsChangedMessage());
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
        }

        public void LockDeviceStorage(string serialNo)
        {
            try
            {
                _log.WriteLine($"Lock device storage ({serialNo})");
                _messenger.Send(new LockDeviceStorageMessage(serialNo));
                _messenger.Send(new ShowInfoNotificationMessage($"Synchronizing credetials in {serialNo} with your other device, please wait" 
                    + Environment.NewLine + "Password manager is temporarily unavailable", notificationId:serialNo));
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }
        }
    }
}
