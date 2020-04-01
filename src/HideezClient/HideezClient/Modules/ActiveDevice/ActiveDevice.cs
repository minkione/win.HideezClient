using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using HideezClient.Models;
using HideezClient.Modules.VaultManager;
using HideezClient.ViewModels;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace HideezClient.Modules
{
    class ActiveDevice : IActiveDevice, IDisposable
    {
        readonly IVaultManager _deviceManager;
        readonly IMessenger _messenger;
        HardwareVaultModel _device;

        readonly object _deviceLock = new object();

        public event ActiveDeviceChangedEventHandler ActiveDeviceChanged;

        public ActiveDevice(IVaultManager deviceManager, IMessenger messenger)
        {
            _deviceManager = deviceManager;
            _messenger = messenger;
            _deviceManager.DevicesCollectionChanged += DeviceManager_DevicesCollectionChanged;
        }

        public HardwareVaultModel Device 
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
                        _messenger.Send(new ActiveDeviceChangedMessage(prevDevice, _device));
                    }
                }
            } 
        }

        void DeviceManager_DevicesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && Device == null)
            {
                // Devices collection changed, set the first available device as active
                Device = _deviceManager.Vaults.FirstOrDefault();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // Change active device to the last added device, if current active device was amongst the removed devices
                if (e.OldItems.Contains(Device))
                    Device = _deviceManager.Vaults.LastOrDefault();
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
