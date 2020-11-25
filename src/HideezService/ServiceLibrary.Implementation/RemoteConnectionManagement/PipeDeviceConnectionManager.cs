using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.PipeDevice;
using HideezMiddleware;
using HideezMiddleware.IPC.DTO;
using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.IPC.IncommingMessages.RemoteDevice;
using HideezMiddleware.IPC.Messages;
using HideezMiddleware.IPC.Messages.RemoteDevice;
using Meta.Lib.Modules.PubSub;
using Newtonsoft.Json;
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
        readonly Dictionary<IMetaPubSub, PipeRemoteDeviceProxy> RemotePipeDevicesDictionary = new Dictionary<IMetaPubSub, PipeRemoteDeviceProxy>();
        readonly DeviceManager _deviceManager;
        readonly IMetaPubSub _messenger;

        public PipeDeviceConnectionManager(DeviceManager deviceManager, IMetaPubSub pubSub, ILog log):
            base(nameof(PipeDeviceConnectionManager), log)
        {
            _deviceManager = deviceManager;
            _messenger = pubSub;

            _messenger.Subscribe<RemoteDeviceDisconnectedMessage>(OnRemoteDeviceDisconnected);
            //_messenger.Subscribe<RemoteConnection_RemoteCommandMessage>(RemoteConnection_RemoteCommandAsync);
            //_messenger.Subscribe<RemoteConnection_VerifyCommandMessage>(RemoteConnection_VerifyCommandAsync);
            //_messenger.Subscribe<RemoteConnection_ResetChannelMessage>(RemoteConnection_ResetChannelAsync);
            _messenger.Subscribe<EstablishRemoteDeviceConnectionMessage>(EstablishRemoteDeviceConnection);
        }

        #region Event Handlers
        async Task EstablishRemoteDeviceConnection(EstablishRemoteDeviceConnectionMessage args)
        {
            try
            {
                //FindDeviceBySerialNo(args.SerialNo, args.ChannelNo);
                var pipeDevice = (PipeRemoteDeviceProxy)FindDeviceBySerialNo(args.SerialNo, args.ChannelNo);
                if (pipeDevice == null)
                {
                    var device = FindDeviceBySerialNo(args.SerialNo, 1) as Device;

                    if (device == null)
                        throw new HideezException(HideezErrorCode.DeviceNotFound, args.SerialNo);

                    pipeDevice = new PipeRemoteDeviceProxy(device, _log);
                    pipeDevice = (PipeRemoteDeviceProxy)_deviceManager.AddDeviceChannelProxy(pipeDevice, device, args.ChannelNo);

                    RemoteDevicePubSubManager remoteDevicePubSub = new RemoteDevicePubSubManager(_messenger, pipeDevice, _log);
                    SubscribeToPipeDeviceEvents(pipeDevice, remoteDevicePubSub.RemoteConnectionPubSub);

                    await _messenger.Publish(new EstablishRemoteDeviceConnectionMessageReply(pipeDevice.Id, remoteDevicePubSub.PipeName, pipeDevice.Name, pipeDevice.Mac));
                }

            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }

        private IDevice FindDeviceBySerialNo(string serialNo, byte channelNo)
        {
            return _deviceManager.Devices.FirstOrDefault(d => d.SerialNo == serialNo && d.ChannelNo == channelNo);
        }

        /// <summary>
        /// Event handler for removing a device when pipe device disconnected.
        /// </summary>
        /// <param name="arg">arg.RemoteConnectionPubSub - key to find pipe device in the dictionary.</param>
        /// <returns></returns>
        Task OnRemoteDeviceDisconnected(RemoteDeviceDisconnectedMessage arg)
        {
            RemotePipeDevicesDictionary.TryGetValue(arg.RemoteConnectionPubSub, out PipeRemoteDeviceProxy pipeDevice);
            if (pipeDevice != null)
            {
                _deviceManager.RemoveDeviceChannel(pipeDevice);
            }
            return Task.CompletedTask;
        }

        async void RemoteConnection_DeviceStateChanged(object sender, byte[] e)
        {
            try
            {
                if (RemotePipeDevicesDictionary.Count > 0)
                {
                    if (sender is IConnectionController connection)
                    {
                        await _messenger.Publish(new RemoteConnection_DeviceStateChangedMessage(connection.Id, e));
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

        void SubscribeToPipeDeviceEvents(PipeRemoteDeviceProxy pipeDevice, IMetaPubSub pubSub)
        {
            //pubSub.Subscribe<RemoteConnection_RemoteCommandMessage>(RemoteConnection_RemoteCommandAsync);
            //pubSub.Subscribe<RemoteConnection_ControlRemoteCommandMessage>(RemoteConnection_ControlRemoteCommandAsync);
            //pubSub.Subscribe<RemoteConnection_VerifyCommandMessage>(RemoteConnection_VerifyCommandAsync);
            //pubSub.Subscribe<RemoteConnection_ResetChannelMessage>(RemoteConnection_ResetChannelAsync);
            RemotePipeDevicesDictionary.Add(pubSub, pipeDevice);
            //pipeDevice.DeviceConnection.DeviceStateChanged += RemoteConnection_DeviceStateChanged;
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
                //pipeDevice.DeviceConnection.DeviceStateChanged -= RemoteConnection_DeviceStateChanged;
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
