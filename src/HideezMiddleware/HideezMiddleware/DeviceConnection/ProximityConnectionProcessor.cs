using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Connection;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Proximity.Interfaces;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.DeviceConnection.Workflow;
using HideezMiddleware.Settings;

namespace HideezMiddleware.DeviceConnection
{

    public sealed class ProximityConnectionProcessor : BaseConnectionProcessor, IDisposable
    {
        readonly IBleConnectionManager _bleConnectionManager;
        readonly IProximitySettingsProviderFactory _proximitySettingsProviderFactory;
        readonly AdvertisementIgnoreList _advIgnoreListMonitor;
        readonly DeviceManager _deviceManager;
        readonly IWorkstationUnlocker _workstationUnlocker;
        readonly IHesAccessManager _hesAccessManager;
        readonly object _lock = new object();

        int _isConnecting = 0;
        bool isRunning = false;

        public ProximityConnectionProcessor(
            ConnectionFlowProcessor connectionFlowProcessor,
            IBleConnectionManager bleConnectionManager,
            IProximitySettingsProviderFactory proximitySettingsProviderFactory,
            AdvertisementIgnoreList advIgnoreListMonitor,
            DeviceManager deviceManager,
            IWorkstationUnlocker workstationUnlocker,
            IHesAccessManager hesAccessManager,
            ILog log) 
            : base(connectionFlowProcessor, nameof(ProximityConnectionProcessor), log)
        {
            _bleConnectionManager = bleConnectionManager ?? throw new ArgumentNullException(nameof(bleConnectionManager));
            _proximitySettingsProviderFactory = proximitySettingsProviderFactory ?? throw new ArgumentNullException(nameof(_proximitySettingsProviderFactory));
            _advIgnoreListMonitor = advIgnoreListMonitor ?? throw new ArgumentNullException(nameof(advIgnoreListMonitor));
            _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
            _workstationUnlocker = workstationUnlocker ?? throw new ArgumentNullException(nameof(workstationUnlocker));
            _hesAccessManager = hesAccessManager ?? throw new ArgumentNullException(nameof(hesAccessManager));
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

        ~ProximityConnectionProcessor()
        {
            Dispose(false);
        }
        #endregion

        public override void Start()
        {
            lock (_lock)
            {
                if (!isRunning)
                {
                    _bleConnectionManager.AdvertismentReceived += BleConnectionManager_AdvertismentReceived;
                    isRunning = true;
                    WriteLine("Started");
                }
            }
        }

        public override void Stop()
        {
            lock (_lock)
            {
                isRunning = false;
                _bleConnectionManager.AdvertismentReceived -= BleConnectionManager_AdvertismentReceived;
                WriteLine("Stopped");
            }
        }

        void SetAccessListFromSettings(ProximitySettings settings)
        {
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

            var id = adv.Id;
            var settingsProvider = _proximitySettingsProviderFactory.GetProximitySettingsProvider(id);
            if (settingsProvider.DisabledUnlockByProximity)
                return;

            var proximity = BleUtils.RssiToProximity(adv.Rssi);
            if (proximity < settingsProvider.UnlockProximity)
                return;

            if (_advIgnoreListMonitor.IsIgnored(id))
                return;

            if (!_hesAccessManager.HasAccessKey())
                return;

            if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0)
            {
                try
                {
                    var device = _deviceManager.Devices.FirstOrDefault(d => d.Id == id && !(d is IRemoteDeviceProxy) && !d.IsBoot);

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
                            var connectionId = new ConnectionId(adv.Id, (byte)DefaultConnectionIdProvider.Csr);
                            await ConnectAndUnlockByConnectionId(connectionId);
                        }
                    }
                    catch (Exception)
                    {
                        // Silent handling. Log is already printed inside of _connectionFlowProcessor.ConnectAndUnlock()
                    }
                    finally
                    {
                        _advIgnoreListMonitor.Ignore(id);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _isConnecting, 0);
                }
            }
        }
    }
}
