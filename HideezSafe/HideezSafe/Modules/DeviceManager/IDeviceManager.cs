using HideezSafe.Models;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace HideezSafe.Modules.DeviceManager
{
    interface IDeviceManager
    {
        event NotifyCollectionChangedEventHandler DevicesCollectionChanged;

        IEnumerable<Device> Devices { get; }
    }
}
