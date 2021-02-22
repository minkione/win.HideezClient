using Hideez.SDK.Communication.Log;
using System;
using Windows.Devices.Enumeration;
using Windows.Foundation;

namespace WinBle
{
    internal class WinBleDeviceWatcher : Logger, IDisposable
    {
        readonly DeviceWatcher _deviceWatcher;
        
        bool _pendingStart;

        public DeviceWatcherStatus Status => _deviceWatcher.Status;

        public event EventHandler<DeviceWatcherStatus> StatusChanged;

        public event TypedEventHandler<DeviceWatcher, DeviceInformation> Added
        {
            add { _deviceWatcher.Added += value; }
            remove { _deviceWatcher.Added -= value; }
        }

        public event TypedEventHandler<DeviceWatcher, DeviceInformationUpdate> Removed
        {
            add { _deviceWatcher.Removed += value; }
            remove { _deviceWatcher.Removed -= value; }
        }

        public WinBleDeviceWatcher(ILog log)
            :base(nameof(WinBleDeviceWatcher), log)
        {
            var aqsFilter = "(System.DeviceInterface.Bluetooth.ServiceGuid:=\"{59AC0001-0F4A-CA95-0849-A6670829557F}\" OR " +
                "System.DeviceInterface.Bluetooth.ServiceGuid:=\"{59AC0000-0F4A-CA95-0849-A6670829557F}\")";

            _deviceWatcher = DeviceInformation.CreateWatcher(aqsFilter);
            _deviceWatcher.Stopped += DeviceWatcher_Stopped;
        }

        #region IDisposable implementation
        bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _deviceWatcher.Stopped -= DeviceWatcher_Stopped;
            }

            _disposed = true;
        }
        #endregion

        void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {
            try
            {
                if (_pendingStart)
                    Start();
                else
                    StatusChanged?.Invoke(this, _deviceWatcher.Status);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        internal void Start()
        {
            if (_deviceWatcher.Status == DeviceWatcherStatus.Created ||
                _deviceWatcher.Status == DeviceWatcherStatus.Stopped ||
                _deviceWatcher.Status == DeviceWatcherStatus.Aborted)
            {
                _pendingStart = false;
                _deviceWatcher.Start();
            }
            else
            {
                _pendingStart = true;
            }

            StatusChanged?.Invoke(this, _deviceWatcher.Status);
        }

        internal void Stop()
        {
            _pendingStart = false;

            if (_deviceWatcher.Status == DeviceWatcherStatus.Started ||
                _deviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted ||
                _deviceWatcher.Status == DeviceWatcherStatus.Aborted)
            {
                _deviceWatcher.Stop();
            }

            StatusChanged?.Invoke(this, _deviceWatcher.Status);
        }
    }
}
