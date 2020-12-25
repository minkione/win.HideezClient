using System;
using System.Collections.Generic;
using System.Linq;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Settings;

namespace HideezMiddleware.DeviceConnection
{
    public class AdvertisementIgnoreList : Logger, IDisposable
    {
        readonly IBleConnectionManager _bleConnectionManager;
        readonly ISettingsManager<WorkstationSettings> _workstationSettingsManager;

        readonly List<string> _ignoreList = new List<string>();
        readonly Dictionary<string, DateTime> _lastAdvRecTime = new Dictionary<string, DateTime>();
        readonly object _lock = new object();
        readonly int _rssiClearDelaySeconds;

        public AdvertisementIgnoreList(
            IBleConnectionManager bleConnectionManager,
            ISettingsManager<WorkstationSettings> workstationSettingsManager,
            int rssiClearDelaySeconds,
            ILog log)
            : base(nameof(AdvertisementIgnoreList), log)
        {
            _bleConnectionManager = bleConnectionManager;
            _workstationSettingsManager = workstationSettingsManager;
            _rssiClearDelaySeconds = rssiClearDelaySeconds;

            _bleConnectionManager.AdvertismentReceived += BleConnectionManager_AdvertismentReceived;
            _bleConnectionManager.AdapterStateChanged += BleConnectionManager_AdapterStateChanged;
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _bleConnectionManager.AdvertismentReceived -= BleConnectionManager_AdvertismentReceived;
                _bleConnectionManager.AdapterStateChanged -= BleConnectionManager_AdapterStateChanged;
                Clear();
            }

            disposed = true;
        }

        ~AdvertisementIgnoreList()
        {
            Dispose(false);
        }
        #endregion

        public void Ignore(string id)
        {
            lock (_lock)
            {
                if (!_ignoreList.Contains(id))
                    _ignoreList.Add(id);

                _lastAdvRecTime[id] = DateTime.UtcNow;
            }
        }

        public bool IsIgnored(string id)
        {
            lock (_lock)
            {
                RemoveTimedOutRecords();

                return _ignoreList.Any(x => x == id);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _ignoreList.Clear();
                _lastAdvRecTime.Clear();
            }
        }

        void BleConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            lock (_lock)
            {
                RemoveTimedOutRecords();

                //var shortMac = BleUtils.ConnectionIdToMac(e.Id);
                if (_ignoreList.Any(x => x == e.Id))
                {
                    var proximity = BleUtils.RssiToProximity(e.Rssi);

                    if (proximity > _workstationSettingsManager.Settings.LockProximity)
                        _lastAdvRecTime[e.Id] = DateTime.UtcNow;
                }
            }
        }

        void BleConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            // TODO: When "Resetting" state is implemented, instead clear list on all changes except "PoweredOn" and "Resetting"
            if (_bleConnectionManager.State == BluetoothAdapterState.PoweredOff || _bleConnectionManager.State == BluetoothAdapterState.Unknown)
                Clear();
        }

        void RemoveTimedOutRecords()
        {
            lock (_lock)
            {
                // Remove MAC's from ignore list if we did not receive an advertisement from them in MAC_IGNORELIST_TIMEOUT_SECONDS seconds
                if (_ignoreList.Count > 0)
                    _ignoreList.RemoveAll(m => (DateTime.UtcNow - _lastAdvRecTime[m]).TotalSeconds >= _rssiClearDelaySeconds);
            }
        }

        public void Remove(string id)
        {
            if (_ignoreList.Count > 0)
                _ignoreList.Remove(id);
        }
    }
}
