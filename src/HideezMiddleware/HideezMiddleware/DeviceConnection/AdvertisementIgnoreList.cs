using HideezMiddleware.Settings;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace HideezMiddleware.DeviceConnection
{
    public class AdvertisementIgnoreList : Logger, IDisposable
    {
        readonly IBleConnectionManager _bleConnectionManager;
        readonly ISettingsManager<WorkstationSettings> _workstationSettingsManager;

        readonly List<string> _ignoreList = new List<string>();
        readonly Dictionary<string, DateTime> _lastAdvRecTime = new Dictionary<string, DateTime>();
        readonly Dictionary<string, Timer> _autoRemovedAdv = new Dictionary<string, Timer>();
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

        /// <summary>
        /// The method of adding the device Id to the ignore list or extending it in the ignore list
        /// for seconds of delay defined in the constructor
        /// </summary>
        /// <param name="id">The device Id</param>
        public void Ignore(string id)
        {
            lock (_lock)
            {
                if (!_ignoreList.Contains(id))
                {
                    _ignoreList.Add(id);
                    WriteLine($"Added new item to advertisment ignore list: {id}");
                }

                _lastAdvRecTime[id] = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// The method of adding the connection Id to the ignore list for defined time.
        /// </summary>
        /// <param name="id">The connection Id.</param>
        /// <param name="lifetime">The time, in miliseconds, in which connection Id will be removed from the ignore list.</param>
        public void IgnoreForTime(string id, int lifetime)
        {
            lock (_lock)
            {
                if (!_ignoreList.Contains(id))
                {
                    _ignoreList.Add(id);
                    WriteLine($"Added new item to advertisment ignore list: {id}");

                    var timer = new Timer(lifetime);

                    void TimeoutCache(object s, ElapsedEventArgs e)
                    {
                        try
                        {
                            timer.Elapsed -= TimeoutCache;
                            timer.Dispose();
                            _autoRemovedAdv.Remove(id);
                            _ignoreList.Remove(id);
                            WriteLine($"Removed item from advertisment ignore list: {id}");
                        }
                        catch(Exception ex)
                        {
                            WriteLine(ex.Message);
                        }
                    }

                    timer.Elapsed += TimeoutCache;
                    timer.Start();
                    _autoRemovedAdv.Add(id, timer);
                }
            }
        }

        /// <summary>
        /// The method of checking that the ignore list contains the connection Id.
        /// </summary>
        /// <param name="id">The connection Id.</param>
        /// <returns>True, if the ignore list contains the connection Id. False, if doesn't.</returns>
        public bool IsIgnored(string id)
        {
            lock (_lock)
            {
                RemoveTimedOutRecords();

                return _ignoreList.Any(x => x == id);
            }
        }

        /// <summary>
        /// Removes all items from the ignore list.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                WriteLine("Clearing advertisment ignore list");
                _ignoreList.Clear();
                _lastAdvRecTime.Clear();

                foreach (var timer in _autoRemovedAdv.Values)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                _autoRemovedAdv.Clear();
            }
        }

        void BleConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            lock (_lock)
            {
                RemoveTimedOutRecords();

                if (_ignoreList.Any(x => x == e.Id))
                {
                    var proximity = BleUtils.RssiToProximity(e.Rssi);

                    if (proximity > _workstationSettingsManager.Settings.LockProximity && !_autoRemovedAdv.ContainsKey(e.Id))
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
                // Remove Id's from ignore list if we did not receive an advertisement from them in _rssiClearDelaySeconds seconds
                if (_ignoreList.Count > 0)
                {
                    var removedItems = _ignoreList.Where(m => _lastAdvRecTime.ContainsKey(m) && 
                    (DateTime.UtcNow - _lastAdvRecTime[m]).TotalSeconds >= _rssiClearDelaySeconds && 
                    !_autoRemovedAdv.ContainsKey(m)).ToList();
                    foreach(var item in removedItems)
                    {
                        _ignoreList.Remove(item);
                        WriteLine($"Removed item from advertisment ignore list: {item}");
                    }
                }
            }
        }

        /// <summary>
        /// The method to remove a specific connection Id from the ignore list.
        /// </summary>
        /// <param name="id">The connection Id.</param>
        public void Remove(string id)
        {
            lock (_lock)
            {
                if (_ignoreList.Count > 0)
                {
                    _ignoreList.Remove(id);
                    WriteLine($"Removed item from advertisment ignore list: {id}");
                }
            }
        }
    }
}
