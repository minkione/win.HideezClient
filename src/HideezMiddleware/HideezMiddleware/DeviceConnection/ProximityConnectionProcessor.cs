using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Settings;

namespace HideezMiddleware.DeviceConnection
{

    public class ProximityConnectionProcessor : Logger, IDisposable
    {
        struct ProximityUnlockAccess
        {
            public string Mac { get; set; }
            public bool CanConnect { get; set; }
        }

        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly IBleConnectionManager _bleConnectionManager;
        readonly ISettingsManager<ProximitySettings> _proximitySettingsManager;
        readonly AdvertisementIgnoreList _advIgnoreListMonitor;
        readonly BleDeviceManager _bleDeviceManager;
        readonly IWorkstationUnlocker _workstationUnlocker;
        readonly object _lock = new object();

        List<string> _macListToConnect;
        ProximitySettings _proximitySettings;

        int _isConnecting = 0;
        bool isRunning = false;

        public event EventHandler<WorkstationUnlockResult> WorkstationUnlockPerformed;

        public ProximityConnectionProcessor(
            ConnectionFlowProcessor connectionFlowProcessor,
            IBleConnectionManager bleConnectionManager,
            ISettingsManager<ProximitySettings> proximitySettingsManager,
            AdvertisementIgnoreList advIgnoreListMonitor,
            BleDeviceManager bleDeviceManager,
            IWorkstationUnlocker workstationUnlocker,
            ILog log) 
            : base(nameof(ProximityConnectionProcessor), log)
        {
            _connectionFlowProcessor = connectionFlowProcessor ?? throw new ArgumentNullException(nameof(connectionFlowProcessor));
            _bleConnectionManager = bleConnectionManager ?? throw new ArgumentNullException(nameof(bleConnectionManager));
            _proximitySettingsManager = proximitySettingsManager ?? throw new ArgumentNullException(nameof(proximitySettingsManager));
            _advIgnoreListMonitor = advIgnoreListMonitor ?? throw new ArgumentNullException(nameof(advIgnoreListMonitor));
            _bleDeviceManager = bleDeviceManager ?? throw new ArgumentNullException(nameof(bleDeviceManager));
            _workstationUnlocker = workstationUnlocker ?? throw new ArgumentNullException(nameof(workstationUnlocker));
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
                _bleConnectionManager.AdvertismentReceived += BleConnectionManager_AdvertismentReceived;
                _proximitySettingsManager.SettingsChanged -= UnlockerSettingsManager_SettingsChanged;
            }

            disposed = true;
        }

        ~ProximityConnectionProcessor()
        {
            Dispose(false);
        }
        #endregion

        public void Start()
        {
            lock (_lock)
            {
                if (!isRunning)
                {
                    _bleConnectionManager.AdvertismentReceived += BleConnectionManager_AdvertismentReceived;
                    _proximitySettingsManager.SettingsChanged += UnlockerSettingsManager_SettingsChanged;
                    SetAccessListFromSettings(_proximitySettingsManager.Settings);
                    isRunning = true;
                    WriteLine("Started");
                }
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                isRunning = false;
                _bleConnectionManager.AdvertismentReceived -= BleConnectionManager_AdvertismentReceived;
                _proximitySettingsManager.SettingsChanged -= UnlockerSettingsManager_SettingsChanged;
                WriteLine("Stopped");
            }
        }

        void SetAccessListFromSettings(ProximitySettings settings)
        {
            _proximitySettings = settings;
            _macListToConnect = _proximitySettings.DevicesProximity.Select(s => s.Mac).ToList();
            _advIgnoreListMonitor.Clear();
        }

        void UnlockerSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<ProximitySettings> e)
        {
            SetAccessListFromSettings(e.NewSettings);
        }

        async void BleConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            await ConnectByProximity(e);
        }

        async Task ConnectByProximity(AdvertismentReceivedEventArgs adv)
        {
            if (!isRunning)
                return;

            if (adv == null)
                return;

            if (_isConnecting == 1)
                return;

            if (_macListToConnect.Count == 0)
                return;

            var mac = BleUtils.ConnectionIdToMac(adv.Id);
            if (!_macListToConnect.Any(m => m == mac))
                return;

            var proximity = BleUtils.RssiToProximity(adv.Rssi);
            var settings = _proximitySettings.GetProximitySettings(mac);
            if (proximity < settings.UnlockProximity)
                return;

            if (_advIgnoreListMonitor.IsIgnored(mac))
                return;

            if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0)
            {
                try
                {
                    var device = _bleDeviceManager.Devices.FirstOrDefault(d => d.Mac == mac && !d.IsRemote && !d.IsBoot);

                    // Unlocked Workstation, Device not found OR Device not connected - dont add to ignore
                    if (!_workstationUnlocker.IsConnected && (device == null || (device != null && !device.IsConnected)))
                        return;

                    try
                    {
                        // Unlocked Workstation, Device connected - add to ignore
                        if (!_workstationUnlocker.IsConnected && device != null && device.IsConnected)
                            return;

                        // Locked Workstation, Device not found OR not connected - connect add to ignore
                        if (_workstationUnlocker.IsConnected && (device == null || (device != null && !device.IsConnected)))
                        {
                            await _connectionFlowProcessor.ConnectAndUnlock(mac, OnUnlockAttempt);
                        }
                    }
                    catch (Exception)
                    {
                        // Silent handling. Log is already printed inside of _connectionFlowProcessor.ConnectAndUnlock()
                    }
                    finally
                    {
                        _advIgnoreListMonitor.Ignore(mac);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _isConnecting, 0);
                }
            }
        }

        void OnUnlockAttempt(WorkstationUnlockResult result)
        {
            if (result.IsSuccessful)
                WorkstationUnlockPerformed?.Invoke(this, result);
        }
    }
}
