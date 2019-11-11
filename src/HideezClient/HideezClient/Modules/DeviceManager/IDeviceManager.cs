using HideezClient.Models;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace HideezClient.Modules.DeviceManager
{
    public interface IDeviceManager
    {
        event NotifyCollectionChangedEventHandler DevicesCollectionChanged;

        IEnumerable<Device> Devices { get; }
    }
}
