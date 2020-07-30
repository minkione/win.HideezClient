using HideezClient.Messages;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages.Remote;
using HideezClient.Extension;
using System;
using System.Threading.Tasks;
using Hideez.SDK.Communication.Log;
using HideezClient.Modules.Log;
using HideezMiddleware.Threading;
using Meta.Lib.Modules.PubSub;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub.Messages;

namespace HideezClient.Modules.ServiceCallbackMessanger
{
    class ServiceCallbackMessanger
    {
        readonly IMessenger _messenger;
        readonly IMetaPubSub _metaMessenger;
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(ServiceCallbackMessanger));
        readonly SemaphoreQueue _sendMessageSemaphore = new SemaphoreQueue(1, 1);

        public ServiceCallbackMessanger(IMessenger messenger, IMetaPubSub metaMessenger)
        {
            _messenger = messenger;
            _metaMessenger = metaMessenger;

            _metaMessenger.TrySubscribeOnServer<ActivateScreenRequestMessage>(ActivateScreenRequest);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.LockWorkstationMessage>(LockWorkstationRequest);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DevicesCollectionChangedMessage>(DevicesCollectionChanged);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceConnectionStateChangedMessage>(DeviceConnectionStateChanged);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceInitializedMessage>(DeviceInitialized);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceFinishedMainFlowMessage>(DeviceFinishedMainFlow);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceOperationCancelledMessage>(DeviceOperationCancelled);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceProximityChangedMessage>(DeviceProximityChanged);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceBatteryChangedMessage>(DeviceBatteryChanged);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.RemoteConnection_DeviceStateChangedMessage>(RemoteConnection_DeviceStateChanged);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.ServiceComponentsStateChangedMessage>(ServiceComponentsStateChanged);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.UserNotificationMessage>(ServiceNotificationReceived);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.UserErrorMessage>(ServiceErrorReceived);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.ShowActivationCodeUiMessage>(ShowActivationCodeUi);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.HideActivationCodeUi>(HideActivationCodeUi);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DeviceProximityLockEnabledMessage>(DeviceProximityLockEnabled);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.WorkstationUnlockedMessage>(WorkstationUnlocked);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.ProximitySettingsChangedMessage>(ProximitySettingsChanged);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.LockDeviceStorageMessage>(LockDeviceStorage);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.LiftDeviceStorageLockMessage>(LiftDeviceStorageLock);

            _metaMessenger.Subscribe<ConnectedToServerEvent>(OnConnectedToServer);
        }

        async Task OnConnectedToServer(ConnectedToServerEvent arg)
        {
            await _metaMessenger.PublishOnServer(new LoginClientRequestMessage());
        }

        public Task ActivateScreenRequest(ActivateScreenRequestMessage message)
        {
            _log.WriteLine($"Activate screen request");
            SendMessage(new ActivateScreenMessage());
            return Task.CompletedTask;
        }

        public Task LockWorkstationRequest(HideezMiddleware.IPC.Messages.LockWorkstationMessage message)
        {
            _log.WriteLine($"Lock workstation request");
            SendMessage(new Messages.LockWorkstationMessage());
            return Task.CompletedTask;
        }

        public Task DevicesCollectionChanged(HideezMiddleware.IPC.Messages.DevicesCollectionChangedMessage message)
        {
            _log.WriteLine($"Paired devices collection changed. Count: {message.Devices.Length}");
            SendMessage(new Messages.DevicesCollectionChangedMessage(message.Devices));
            return Task.CompletedTask;
        }

        public Task DeviceConnectionStateChanged(HideezMiddleware.IPC.Messages.DeviceConnectionStateChangedMessage message)
        {
            _log.WriteLine($"({message.Device.Id}) Vault connection state changed to: {message.Device.IsConnected}");
            SendMessage(new Messages.DeviceConnectionStateChangedMessage(message.Device));
            return Task.CompletedTask;
        }

        public Task DeviceInitialized(HideezMiddleware.IPC.Messages.DeviceInitializedMessage message)
        {
            _log.WriteLine($"({message.Device.Id}) Vault is initialized");
            SendMessage(new Messages.DeviceInitializedMessage(message.Device));
            return Task.CompletedTask;
        }

        public Task DeviceFinishedMainFlow(HideezMiddleware.IPC.Messages.DeviceFinishedMainFlowMessage message)
        {
            _log.WriteLine($"({message.Device.Id}) Vault has finished main flow");
            SendMessage(new Messages.DeviceFinishedMainFlowMessage(message.Device));
            return Task.CompletedTask;
        }

        public Task DeviceOperationCancelled(HideezMiddleware.IPC.Messages.DeviceOperationCancelledMessage message)
        {
            _log.WriteLine($"({message.Device.Id}) Vault operation cancelled");
            SendMessage(new Messages.DeviceOperationCancelledMessage(message.Device));
            return Task.CompletedTask;
        }

        public Task DeviceProximityChanged(HideezMiddleware.IPC.Messages.DeviceProximityChangedMessage message)
        {
            _log.WriteLine($"({message.DeviceId}) DevVaultice proximity changed to {message.Proximity}");
            SendMessage(new Messages.DeviceProximityChangedMessage(message.DeviceId, message.Proximity));
            return Task.CompletedTask;
        }

        public Task DeviceBatteryChanged(HideezMiddleware.IPC.Messages.DeviceBatteryChangedMessage message)
        {
            _log.WriteLine($"({message.DeviceId}) Vault battery changed to {message.Battery}");
            SendMessage(new Messages.DeviceBatteryChangedMessage(message.DeviceId, message.Battery));
            return Task.CompletedTask;
        }

        public Task RemoteConnection_DeviceStateChanged(RemoteConnection_DeviceStateChangedMessage message)
        {
            //_log.WriteLine($"({deviceId}) Remote system state received");
            SendMessage(new Remote_DeviceStateChangedMessage(message.DeviceId, message.State.ToDeviceState()));
            return Task.CompletedTask;
        }

        public Task ServiceComponentsStateChanged(HideezMiddleware.IPC.Messages.ServiceComponentsStateChangedMessage message)
        {
            _log.WriteLine($"Service components state changed (hes:{message.HesStatus}; rfid:{message.RfidStatus}; ble:{message.BluetoothStatus}; tbHes:{message.TbHesStatus};)");
            SendMessage(new Messages.ServiceComponentsStateChangedMessage(message.HesStatus, message.RfidStatus, message.BluetoothStatus, message.TbHesStatus));
            return Task.CompletedTask;
        }

        public Task ServiceNotificationReceived(UserNotificationMessage message)
        {
            _log.WriteLine($"Notification message from service: {message.Message} ({message.NotificationId})");
            SendMessage(new ServiceNotificationReceivedMessage(message.NotificationId, message.Message));
            return Task.CompletedTask;
        }

        public Task ServiceErrorReceived(UserErrorMessage message)
        {
            _log.WriteLine($"Error message from service: {message.Message} ({message.NotificationId})");
            SendMessage(new ServiceErrorReceivedMessage(message.NotificationId, message.Message));
            return Task.CompletedTask;
        }

        public Task ShowActivationCodeUi(HideezMiddleware.IPC.Messages.ShowActivationCodeUiMessage message)
        {
            _log.WriteLine($"Show activation code ui message for ({message.DeviceId})");
            SendMessage(new Messages.ShowActivationCodeUiMessage(message.DeviceId));
            return Task.CompletedTask;
        }

        public Task HideActivationCodeUi(HideActivationCodeUi message)
        {
            _log.WriteLine($"Hide activation code ui message");
            SendMessage(new HideActivationCodeUiMessage());
            return Task.CompletedTask;
        }

        public Task DeviceProximityLockEnabled(HideezMiddleware.IPC.Messages.DeviceProximityLockEnabledMessage message)
        {
            _log.WriteLine($"({message.Device.Id}) Vault marked as valid for workstation lock");
            SendMessage(new Messages.DeviceProximityLockEnabledMessage(message.Device));
            return Task.CompletedTask;
        }

        public Task WorkstationUnlocked(HideezMiddleware.IPC.Messages.WorkstationUnlockedMessage message)
        {
            SendMessage(new UnlockWorkstationMessage(message.IsNotHideezMethod));
            return Task.CompletedTask;
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

        public Task ProximitySettingsChanged(HideezMiddleware.IPC.Messages.ProximitySettingsChangedMessage message)
        {
            try
            {
                _log.WriteLine($"Vault proximity settings changed");
                _messenger.Send(new DeviceProximitySettingsChangedMessage());
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }

            return Task.CompletedTask;
        }

        public Task LockDeviceStorage(HideezMiddleware.IPC.Messages.LockDeviceStorageMessage message)
        {
            try
            {
                _log.WriteLine($"Lock vault storage ({message.SerialNo})");
                _messenger.Send(new Messages.LockDeviceStorageMessage(message.SerialNo));
                _messenger.Send(new ShowInfoNotificationMessage($"Synchronizing credentials in {message.SerialNo} with your other vault, please wait" 
                    + Environment.NewLine + "Password manager is temporarily unavailable", notificationId:message.SerialNo));
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }

            return Task.CompletedTask;
        }

        public Task LiftDeviceStorageLock(HideezMiddleware.IPC.Messages.LiftDeviceStorageLockMessage message)
        {
            try
            {
                _log.WriteLine($"Lift vault storage lock ({message.SerialNo})");
                _messenger.Send(new Messages.LiftDeviceStorageLockMessage(message.SerialNo));
            }
            catch (Exception ex)
            {
                _log.WriteLine(ex);
            }

            return Task.CompletedTask;
        }
    }
}
