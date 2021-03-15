using Meta.Lib.Modules.PubSub;
using System.Threading.Tasks;
using HideezMiddleware.IPC.Messages;
using System.Collections.Generic;
using HideezClient.Messages;
using HideezMiddleware.Localize;
using HideezClient.Models;
using HideezClient.Modules.DeviceManager;
using System.Linq;
using Hideez.SDK.Communication.BLE;

namespace HideezClient.Modules.VaultLowBatteryMonitor
{
    internal class VaultLowBatteryMonitor : IVaultLowBatteryMonitor
    {
        readonly IMetaPubSub _messenger;
        readonly IDeviceManager _deviceManager;
        readonly HashSet<string> vaultIdFilter = new HashSet<string>();
        readonly object filterLock = new object();

        const string NOTIFICATION_ID_SUFFIX = "_specialBatteryNotification"; // Doesn't matter what it is, as long as its a unique constant string

        public VaultLowBatteryMonitor(IMetaPubSub messenger, IDeviceManager deviceManager)
        {
            _messenger = messenger;
            _deviceManager = deviceManager;

            _messenger.TrySubscribeOnServer<DeviceBatteryChangedMessage>(OnDeviceBatteryChanged);
            _messenger.TrySubscribeOnServer<DeviceConnectionStateChangedMessage>(OnDeviceConnectionStateChanged);
        }

        Task OnDeviceBatteryChanged(DeviceBatteryChangedMessage obj)
        {
            lock (filterLock)
            {
                // Todo: Might need a better algorithm for detecting low battery
                if (!vaultIdFilter.Contains(obj.Mac) && obj.Battery != 0 && obj.Battery <= 25)
                {
                    var device = _deviceManager.Devices.FirstOrDefault(d => d.Mac == obj.Mac);
                    if (device != null && device.IsConnected && device.FinishedMainFlow)
                    {
                        vaultIdFilter.Add(obj.Mac);

                        _messenger.Publish(new ShowLowBatteryNotificationMessage(
                            TranslationSource.Instance["LowBatteryNotification.Message"],
                            TranslationSource.Instance["LowBatteryNotification.Title"],
                            new NotificationOptions { CloseTimeout = NotificationOptions.NoTimeout },
                            obj.DeviceId + NOTIFICATION_ID_SUFFIX));
                    }
                }
            }

            return Task.CompletedTask;
        }

        Task OnDeviceConnectionStateChanged(DeviceConnectionStateChangedMessage obj)
        {
            lock (filterLock)
            {
                vaultIdFilter.Remove(obj.Device.Id);
            }

            return Task.CompletedTask;
        }
    }
}
