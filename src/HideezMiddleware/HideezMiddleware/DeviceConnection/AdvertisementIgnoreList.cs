using Hideez.SDK.Communication.BLE;
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
        readonly ISettingsManager<UnlockerSettings> _unlockerSettingsManager;

        readonly List<string> _ignoreList = new List<string>();
        readonly Dictionary<string, DateTime> _lastAdvRecTime = new Dictionary<string, DateTime>();
        readonly object _lock = new object();

        public AdvertisementIgnoreList(
            IBleConnectionManager bleConnectionManager,
            BleDeviceManager bleDeviceManager,
            ISettingsManager<UnlockerSettings> unlockerSettingsManager,
            ILog log) 
            : base(nameof(AdvertisementIgnoreList), log)
        {
            _bleConnectionManager = bleConnectionManager;
            _bleDeviceManager = bleDeviceManager;
            _unlockerSettingsManager = unlockerSettingsManager;

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

        public bool IsIgnored(string mac)
        {
            lock(_lock)
            {
                // Remove MAC's from ignore list if we did not receive an advertisement from them in MAC_IGNORELIST_TIMEOUT_SECONDS seconds
                _ignoreList.RemoveAll(m => (DateTime.UtcNow - _lastAdvRecTime[m].Date).Seconds >= MAC_IGNORELIST_TIMEOUT_SECONDS);

                return _ignoreList.Any(m => m == mac);
            }
        }

        void BleConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            lock(_lock)
            {
                var mac = MacUtils.GetMacFromShortMac(e.Id);
                if (_ignoreList.Any(m => m == mac))
                {
                    var proximity = BleUtils.RssiToProximity(e.Rssi);

                    if (proximity <= _unlockerSettingsManager.Settings.LockProximity)
                        _ignoreList.Remove(mac);
                    else
                        _lastAdvRecTime[mac] = DateTime.UtcNow;
                }
            }
        }

    }
}
