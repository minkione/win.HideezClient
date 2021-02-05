using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Modules.DeviceManagement.Messages;
using HideezMiddleware.Modules.Hes.Messages;
using HideezMiddleware.Threading;
using Meta.Lib.Modules.PubSub;
using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware.Modules.DeviceManagement
{
    public sealed class DeviceManagementModule : ModuleBase
    {
        readonly DeviceManager _deviceManager;

        public DeviceManagementModule(DeviceManager deviceManager, IMetaPubSub messenger, ILog log)
            : base(messenger, nameof(DeviceManagementModule), log)
        {
            _deviceManager = deviceManager;

            _deviceManager.DeviceAdded += DevicesManager_DeviceAdded;
            _deviceManager.DeviceRemoved += DevicesManager_DeviceRemoved;

            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;

            _messenger.Subscribe<HesAccessManager_AccessRetractedMessage>(HesAccessManager_AccessRetracted);
        }

        private readonly SemaphoreQueue _devicesSemaphore = new SemaphoreQueue(1, 1);
        private async void DevicesManager_DeviceAdded(object sender, DeviceAddedEventArgs e)
        {
            await _devicesSemaphore.WaitAsync();
            try
            {
                await SafePublish(new DeviceManager_DeviceAddedMessage(_deviceManager, e.Device));

                if (e.Device != null)
                {
                    // TODO: check, who is responsible for handling these events
                    e.Device.ConnectionStateChanged += Device_ConnectionStateChanged;
                    e.Device.Initialized += Device_Initialized;
                    e.Device.Disconnected += Device_Disconnected;
                    e.Device.OperationCancelled += Device_OperationCancelled;
                    e.Device.ProximityChanged += Device_ProximityChanged;
                    e.Device.BatteryChanged += Device_BatteryChanged;
                    e.Device.WipeFinished += Device_WipeFinished;
                    e.Device.AccessLevelChanged += Device_AccessLevelChanged;
                }
            }
            finally
            {
                _devicesSemaphore.Release();
            }
        }

        private async void DevicesManager_DeviceRemoved(object sender, DeviceRemovedEventArgs e)
        {
            await _devicesSemaphore.WaitAsync();
            try
            {
                await SafePublish(new DeviceManager_DeviceRemovedMessage(_deviceManager, e.Device));

                if (e.Device != null)
                {
                    // TODO: check, who is responsible for handling these events
                    e.Device.ConnectionStateChanged -= Device_ConnectionStateChanged;
                    e.Device.Initialized -= Device_Initialized;
                    e.Device.Disconnected -= Device_Disconnected;
                    e.Device.OperationCancelled -= Device_OperationCancelled;
                    e.Device.ProximityChanged -= Device_ProximityChanged;
                    e.Device.BatteryChanged -= Device_BatteryChanged;
                    e.Device.WipeFinished -= Device_WipeFinished;
                    e.Device.AccessLevelChanged -= Device_AccessLevelChanged;
                }

                // Deivce Added/Removed and handled by client pipe
                //if (device is PipeRemoteDeviceProxy pipeDevice)
                //    _pipeDeviceConnectionManager.RemovePipeDeviceConnection(pipeDevice);

                // handled by audit
                //if (!(device is IRemoteDeviceProxy) && device.IsInitialized)
                //{
                //    var workstationEvent = _eventSaver.GetWorkstationEvent();
                //    workstationEvent.EventId = WorkstationEventType.DeviceDeleted;
                //    workstationEvent.Severity = WorkstationEventSeverity.Warning;
                //    workstationEvent.DeviceId = device.SerialNo;
                //    await _eventSaver.AddNewAsync(workstationEvent);
                //}

                // handled by client pipe
                //_deviceDTOFactory.ResetCounter(device);
            }
            finally
            {
                _devicesSemaphore.Release();
            }
        }

        private async void Device_ConnectionStateChanged(object sender, EventArgs e)
        {
            //try
            //{
            //    if (sender is IDevice device)
            //    {
            //        if (!device.IsConnected)
            //            device.SetUserProperty(CustomProperties.HW_CONNECTION_STATE_PROP, HwVaultConnectionState.Offline);

            //        await SafePublish(new DeviceConnectionStateChangedMessage(_deviceDTOFactory.Create(device)));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Error(ex);
            //}
        }

        private async void Device_Initialized(object sender, EventArgs e)
        {
            //try
            //{
            //    if (sender is IDevice device)
            //    {
            //        // Separate error handling block for each callback ensures we try to notify 
            //        // every session, even if an error occurs
            //        try
            //        {
            //            await SafePublish(new DeviceInitializedMessage(_deviceDTOFactory.Create(device)));
            //        }
            //        catch (Exception ex)
            //        {
            //            Error(ex);
            //        }

            //        if (!(device is IRemoteDeviceProxy) || device.ChannelNo > 2)
            //        {
            //            var workstationEvent = _eventSaver.GetWorkstationEvent();
            //            workstationEvent.Severity = WorkstationEventSeverity.Info;
            //            workstationEvent.DeviceId = device.SerialNo;
            //            if (device is IRemoteDeviceProxy)
            //            {
            //                workstationEvent.EventId = WorkstationEventType.RemoteConnect;
            //            }
            //            else
            //            {
            //                workstationEvent.EventId = WorkstationEventType.DeviceConnect;
            //            }
            //            await _eventSaver.AddNewAsync(workstationEvent);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Error(ex);
            //}
        }

        private async void Device_Disconnected(object sender, EventArgs e)
        {
            //if (sender is IDevice device && device.IsInitialized && (!(device is IRemoteDeviceProxy) || device.ChannelNo > 2))
            //{
            //    var workstationEvent = _eventSaver.GetWorkstationEvent();
            //    workstationEvent.Severity = WorkstationEventSeverity.Info;
            //    workstationEvent.DeviceId = device.SerialNo;
            //    if (device is IRemoteDeviceProxy)
            //    {
            //        workstationEvent.EventId = WorkstationEventType.RemoteDisconnect;
            //    }
            //    else
            //    {
            //        workstationEvent.EventId = WorkstationEventType.DeviceDisconnect;
            //    }
            //    await _eventSaver.AddNewAsync(workstationEvent);
            //}
        }

        private async void Device_OperationCancelled(object sender, EventArgs e)
        {
            //if (sender is IDevice device)
            //    await SafePublish(new DeviceOperationCancelledMessage(_deviceDTOFactory.Create(device)));
        }

        private async void Device_ProximityChanged(object sender, double e)
        {
            //if (sender is IDevice device)
            //    await SafePublish(new DeviceProximityChangedMessage(device.Id, e));
        }

        private async void Device_BatteryChanged(object sender, sbyte e)
        {
            //if (sender is IDevice device)
            //    await SafePublish(new DeviceBatteryChangedMessage(device.Id, device.Mac, e));
        }

        private void Device_WipeFinished(object sender, WipeFinishedEventtArgs e)
        {
            //if (e.Status == FwWipeStatus.WIPE_OK)
            //{
            //    var device = (IDevice)sender;
            //    _log?.WriteLine($"({device.SerialNo}) Wipe finished. Disabling automatic reconnect");
            //    _deviceReconnectManager.DisableDeviceReconnect(device);
            //    device.Disconnected += async (s, a) =>
            //    {
            //        try
            //        {
            //            // Wiped device is cleared of all bond information, and therefore must be paired again
            //            await _deviceManager.DeleteBond(device.DeviceConnection);
            //        }
            //        catch (Exception ex)
            //        {
            //            Error(ex);
            //        }
            //    };

            //}
        }

        private async void Device_AccessLevelChanged(object sender, AccessLevel e)
        {
            //var device = (IDevice)sender;
            //if (device.ChannelNo == (int)DefaultDeviceChannel.Main)
            //{
            //    try
            //    {
            //        await device.RefreshDeviceInfo();
            //    }
            //    catch { }
            //}
        }


        // Disconnect all devices when user is logged out or session is locked
        private async void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            try
            {
                if (reason == SessionSwitchReason.SessionLogoff || reason == SessionSwitchReason.SessionLock)
                {
                    // Disconnect all connected devices
                    foreach (var device in _deviceManager.Devices)
                        await _deviceManager.Disconnect(device.DeviceConnection);
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        // Disconnect all devices when access is retracted
        private async Task HesAccessManager_AccessRetracted(HesAccessManager_AccessRetractedMessage msg)
        {
            try
            {
                foreach (var device in _deviceManager.Devices)
                {
                    await _deviceManager.Disconnect(device.DeviceConnection).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }
    }
}
