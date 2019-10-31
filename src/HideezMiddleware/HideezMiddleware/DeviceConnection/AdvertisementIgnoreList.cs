using System;
using System.Collections.Generic;
using System.Linq;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Settings;

namespace HideezMiddleware.DeviceConnection
{
    public class AdvertisementIgnoreList : Logger, IDisposable
    {
        const int MAC_IGNORELIST_TIMEOUT_SECONDS = 3;

        readonly IBleConnectionManager _bleConnectionManager;
        readonly ISettingsManager<ProximitySettings> _proximitySettingsManager;

        readonly List<string> _ignoreList = new List<string>();
        readonly Dictionary<string, DateTime> _lastAdvRecTime = new Dictionary<string, DateTime>();
        readonly object _lock = new object();

        public AdvertisementIgnoreList(
            IBleConnectionManager bleConnectionManager,
            ISettingsManager<ProximitySettings> proximitySettingsManager,
            ILog log)
            : base(nameof(AdvertisementIgnoreList), log)
        {
            _bleConnectionManager = bleConnectionManager;
            _proximitySettingsManager = proximitySettingsManager;

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

        public void Ignore(string mac)
        {
            lock (_lock)
            {
                if (!_ignoreList.Contains(mac))
                    _ignoreList.Add(mac);

                _lastAdvRecTime[mac] = DateTime.UtcNow;
            }
        }

        public bool IsIgnored(string mac)
        {
            lock (_lock)
            {
                RemoveTimedOutRecords();

                return _ignoreList.Any(m => m == BleUtils.ConnectionIdToMac(mac));
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

                var shortMac = BleUtils.ConnectionIdToMac(e.Id);
                if (_ignoreList.Any(m => m == shortMac))
                {
                    var proximity = BleUtils.RssiToProximity(e.Rssi);

                    var settings = _proximitySettingsManager.Settings.GetProximitySettings(shortMac);
                    if (proximity <= settings.LockProximity)
                        _ignoreList.Remove(shortMac);
                    else
                        _lastAdvRecTime[shortMac] = DateTime.UtcNow;
                }
            }
        }

        void BleConnectionManager_AdapterStateChanged(object sender, EventArgs e)
        {
            Clear();
        }

        void RemoveTimedOutRecords()
        {
            lock (_lock)
            {
                // Remove MAC's from ignore list if we did not receive an advertisement from them in MAC_IGNORELIST_TIMEOUT_SECONDS seconds
                if (_ignoreList.Count > 0)
                    _ignoreList.RemoveAll(m => (DateTime.UtcNow - _lastAdvRecTime[m]).Seconds >= MAC_IGNORELIST_TIMEOUT_SECONDS);
            }
        }

    }
}
