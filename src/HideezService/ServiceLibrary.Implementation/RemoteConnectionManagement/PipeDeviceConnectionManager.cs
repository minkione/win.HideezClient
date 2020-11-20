using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.PipeDevice;
using HideezMiddleware;
using HideezMiddleware.IPC.DTO;
using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.IPC.IncommingMessages.RemoteDevice;
using HideezMiddleware.IPC.Messages;
using HideezMiddleware.IPC.Messages.RemoteDevice;
using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation.RemoteConnectionManagement
{
    /// <summary>
    /// Class to manage pipe device connection.
    /// </summary>
    class PipeDeviceConnectionManager: Logger
    {
        readonly Dictionary<IMetaPubSub, IPipeDevice> RemotePipeDevicesDictionary = new Dictionary<IMetaPubSub, IPipeDevice>();
        readonly BleDeviceManager _deviceManager;
        readonly IMetaPubSub _messenger;
        readonly PipeDeviceFactory _pipeDeviceFactory;

        public PipeDeviceConnectionManager(BleDeviceManager deviceManager, IMetaPubSub pubSub, ILog log):
            base(nameof(PipeDeviceConnectionManager), log)
        {
            _deviceManager = deviceManager;
            _messenger = pubSub;
            _pipeDeviceFactory = new PipeDeviceFactory(_deviceManager, log);

            _messenger.Subscribe<RemoteDeviceDisconnectedMessage>(OnRemoteDeviceDisconnected);
            _messenger.Subscribe<RemoteConnection_RemoteCommandMessage>(RemoteConnection_RemoteCommandAsync);
            _messenger.Subscribe<RemoteConnection_VerifyCommandMessage>(RemoteConnection_VerifyCommandAsync);
            _messenger.Subscribe<RemoteConnection_ResetChannelMessage>(RemoteConnection_ResetChannelAsync);
            _messenger.Subscribe<EstablishRemoteDeviceConnectionMessage>(EstablishRemoteDeviceConnection);
        }

        #region Event Handlers
        async Task EstablishRemoteDeviceConnection(EstablishRemoteDeviceConnectionMessage args)
        {
            try
            {
                RemoteDevicePubSubManager remoteDevicePubSub = new RemoteDevicePubSubManager(_messenger, _log);

                var pipeDevice = (IPipeDevice)_deviceManager.FindBySerialNo(args.SerialNo, args.ChannelNo);
                if (pipeDevice == null)
                {
                    var device = _deviceManager.FindBySerialNo(args.SerialNo, 1);

                    if (device == null)
                        throw new HideezException(HideezErrorCode.DeviceNotFound, args.SerialNo);

                    pipeDevice = await _pipeDeviceFactory.EstablishRemoteDeviceConnection(device.Mac, args.ChannelNo);

                    SubscribeToPipeDeviceEvents(pipeDevice, remoteDevicePubSub.RemoteConnectionPubSub);
                }

                await _messenger.Publish(new EstablishRemoteDeviceConnectionMessageReply(pipeDevice.Id, remoteDevicePubSub.PipeName));
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Event handler for removing a device when pipe device disconnected.
        /// </summary>
        /// <param name="arg">arg.RemoteConnectionPubSub - key to find pipe device in the dictionary.</param>
        /// <returns></returns>
        Task OnRemoteDeviceDisconnected(RemoteDeviceDisconnectedMessage arg)
        {
            RemotePipeDevicesDictionary.TryGetValue(arg.RemoteConnectionPubSub, out IPipeDevice pipeDevice);
            if (pipeDevice != null)
            {
                _deviceManager.Remove(pipeDevice);
            }
            return Task.CompletedTask;
        }

        async Task RemoteConnection_VerifyCommandAsync(RemoteConnection_VerifyCommandMessage args)
        {
            try
            {
                var pipeDevice = _deviceManager.Find(args.ConnectionId) as IPipeDevice;

                if (pipeDevice == null)
                    throw new HideezException(HideezErrorCode.RemoteDeviceNotFound, args.ConnectionId);

                var response = await pipeDevice.OnVerifyCommandAsync(args.Data);

                await _messenger.Publish(new RemoteConnection_VerifyCommandMessageReply(response));
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        async Task RemoteConnection_RemoteCommandAsync(RemoteConnection_RemoteCommandMessage args)
        {
            try
            {
                var pipeDevice = _deviceManager.Find(args.ConnectionId) as IPipeDevice;

                if (pipeDevice == null)
                    throw new HideezException(HideezErrorCode.RemoteDeviceNotFound, args.ConnectionId);

                var response = await pipeDevice.OnRemoteCommandAsync(args.Data);

                await _messenger.Publish(new RemoteConnection_RemoteCommandMessageReply(response));
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        async Task RemoteConnection_ResetChannelAsync(RemoteConnection_ResetChannelMessage args)
        {
            try
            {
                var pipeDevice = _deviceManager.Find(args.ConnectionId) as IPipeDevice;

                if (pipeDevice == null)
                    throw new HideezException(HideezErrorCode.RemoteDeviceNotFound, args.ConnectionId);

                await pipeDevice.OnResetChannelAsync();
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        async void RemoteConnection_DeviceStateChanged(object sender, DeviceStateEventArgs e)
        {
            try
            {
                if (RemotePipeDevicesDictionary.Count > 0)
                {
                    if (sender is IPipeDevice pipeDevice)
                    {
                        await _messenger.Publish(new RemoteConnection_DeviceStateChangedMessage(pipeDevice.Id, new DeviceStateDTO(e.State)));
                    }
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }
        #endregion

        /// <summary>
        /// Remove the pipe device.
        /// </summary>
        /// <param name="pipeDevice">The pipe device, that must be removed.</param>
        public void RemovePipeDevice(IPipeDevice pipeDevice)
        {
            var pubSub = RemotePipeDevicesDictionary.Where(p => p.Value == pipeDevice).FirstOrDefault().Key;
            DisposePipeDevicePair(pipeDevice, pubSub);
        }

        void SubscribeToPipeDeviceEvents(IPipeDevice pipeDevice, IMetaPubSub pubSub)
        {
            RemotePipeDevicesDictionary.Add(pubSub, pipeDevice);
            pipeDevice.DeviceStateChanged += RemoteConnection_DeviceStateChanged;
        }

        /// <summary>
        /// The method that unsubscribes the pipe device from all events, stops direct MetaPubSub server 
        /// and remove its pairing with the relative pipe device.
        /// </summary>
        /// <param name="pipeDevice">The pipe device that must unsubscribe from all events.</param>
        /// <param name="pubSub">The key through which you want to remove the pipe device from the dictionary.</param>
        void DisposePipeDevicePair(IPipeDevice pipeDevice, IMetaPubSub pubSub)
        {
            if (pubSub != null)
            {
                pipeDevice.DeviceStateChanged -= RemoteConnection_DeviceStateChanged;
                pubSub.StopServer();
                RemotePipeDevicesDictionary.Remove(pubSub);
            }
        }

        void Error(Exception ex, string message = "")
        {
            _log?.WriteLine(message, ex);
        }
    }
}
