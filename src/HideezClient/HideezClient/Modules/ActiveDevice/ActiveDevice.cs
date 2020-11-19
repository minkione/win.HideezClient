using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using HideezClient.Models;
using HideezClient.Modules.DeviceManager;
using HideezClient.ViewModels;
using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace HideezClient.Modules
{
    class ActiveDevice : IActiveDevice, IDisposable
    {
        readonly IDeviceManager _deviceManager;
        readonly IMetaPubSub _metaMessenger;
        DeviceModel _device;

        readonly object _deviceLock = new object();

        public event ActiveDeviceChangedEventHandler ActiveDeviceChanged;

        public ActiveDevice(IDeviceManager deviceManager, IMetaPubSub metaMessenger)
        {
            _deviceManager = deviceManager;
            _metaMessenger = metaMessenger;
            _deviceManager.DevicesCollectionChanged += DeviceManager_DevicesCollectionChanged;
        }

        public DeviceModel Device 
        {
            get { return _device; }
            set
            {
                lock (_deviceLock)
                {
                    if (_device != value)
                    {
                        var prevDevice = _device;
                        _device = value;
                        ActiveDeviceChanged?.Invoke(this, new ActiveDeviceChangedEventArgs(prevDevice, _device));
                        _metaMessenger.Publish(new ActiveDeviceChangedMessage(prevDevice, _device));
                    }
                }
            } 
        }

        void DeviceManager_DevicesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && Device == null)
            {
                // Devices collection changed, set the first available device as active
                Device = _deviceManager.Devices.FirstOrDefault();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // Change active device to the last added device, if current active device was amongst the removed devices
                if (e.OldItems.Contains(Device))
                    Device = _deviceManager.Devices.LastOrDefault();
            }
        }

        #region IDisposable Support
        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _deviceManager.DevicesCollectionChanged -= DeviceManager_DevicesCollectionChanged;
                }

                disposed = true;
            }
        }

        ~ActiveDevice()
        {
            Dispose(false);
        }
        #endregion
    }
}
