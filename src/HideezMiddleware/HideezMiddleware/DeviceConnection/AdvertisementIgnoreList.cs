using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Settings;
using HideezMiddleware.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HideezMiddleware.DeviceConnection
{
    public class AdvertisementIgnoreList : Logger, IDisposable
    {
        const int MAC_IGNORELIST_TIMEOUT_SECONDS = 3;

        readonly IBleConnectionManager _bleConnectionManager;
        readonly BleDeviceManager _bleDeviceManager;
        readonly ISettingsManager<ProximitySettings> _proximitySettingsManager;

        readonly List<string> _ignoreList = new List<string>();
        readonly Dictionary<string, DateTime> _lastAdvRecTime = new Dictionary<string, DateTime>();
        readonly object _lock = new object();

        public AdvertisementIgnoreList(
            IBleConnectionManager bleConnectionManager,
            BleDeviceManager bleDeviceManager,
            ISettingsManager<ProximitySettings> proximitySettingsManager,
            ILog log)
            : base(nameof(AdvertisementIgnoreList), log)
        {
            _bleConnectionManager = bleConnectionManager;
            _bleDeviceManager = bleDeviceManager;
            _proximitySettingsManager = proximitySettingsManager;

            _bleConnectionManager.AdvertismentReceived += BleConnectionManager_AdvertismentReceived;
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _bleConnectionManager.AdvertismentReceived -= BleConnectionManager_AdvertismentReceived;
            }

            disposed = true;
        }

        ~AdvertisementIgnoreList()
        {
            Dispose(false);
        }
        #endregion

        // Todo: Should be a private member
        public void SetIgnoreList(string[] macArray)
        {
            lock (_lock)
            {
                _ignoreList.Clear();
                _lastAdvRecTime.Clear();
                _ignoreList.AddRange(macArray);
            }
        }

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
                // Remove MAC's from ignore list if we did not receive an advertisement from them in MAC_IGNORELIST_TIMEOUT_SECONDS seconds
                _ignoreList.RemoveAll(m => (DateTime.UtcNow - _lastAdvRecTime[m]).Seconds >= MAC_IGNORELIST_TIMEOUT_SECONDS);

                return _ignoreList.Any(m => m == mac);
            }
        }

        void BleConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            lock (_lock)
            {
                var mac = MacUtils.GetMacFromShortMac(e.Id);
                if (_ignoreList.Any(m => m == mac))
                {
                    var proximity = BleUtils.RssiToProximity(e.Rssi);

                    var settings = _proximitySettingsManager.Settings.GetProximitySettings(mac);
                    if (proximity <= settings.LockProximity)
                        _ignoreList.Remove(mac);
                    else
                        _lastAdvRecTime[mac] = DateTime.UtcNow;
                }
            }
        }

    }
}
