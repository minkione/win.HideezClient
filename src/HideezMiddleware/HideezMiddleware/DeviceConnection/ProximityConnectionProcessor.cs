using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Settings;
using HideezMiddleware.Utils;

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
        readonly IScreenActivator _screenActivator;
        readonly IClientUiManager _clientUiManager;
        readonly ISettingsManager<ProximitySettings> _proximitySettingsManager;
        readonly AdvertisementIgnoreList _advIgnoreListMonitor;
        readonly BleDeviceManager _bleDeviceManager;
        readonly IWorkstationUnlocker _workstationUnlocker;

        List<string> MacListToConnect { get; set; }

        int _isConnecting = 0;

        public ProximityConnectionProcessor(
            ConnectionFlowProcessor connectionFlowProcessor,
            IBleConnectionManager bleConnectionManager,
            IScreenActivator screenActivator,
            IClientUiManager clientUi,
            ISettingsManager<ProximitySettings> proximitySettingsManager,
            AdvertisementIgnoreList advIgnoreListMonitor,
            BleDeviceManager bleDeviceManager,
            IWorkstationUnlocker workstationUnlocker,
            ILog log) 
            : base(nameof(ProximityConnectionProcessor), log)
        {
            _connectionFlowProcessor = connectionFlowProcessor ?? throw new ArgumentNullException(nameof(connectionFlowProcessor));
            _bleConnectionManager = bleConnectionManager ?? throw new ArgumentNullException(nameof(bleConnectionManager));
            _clientUiManager = clientUi ?? throw new ArgumentNullException(nameof(clientUi));
            _proximitySettingsManager = proximitySettingsManager ?? throw new ArgumentNullException(nameof(proximitySettingsManager));
            _advIgnoreListMonitor = advIgnoreListMonitor ?? throw new ArgumentNullException(nameof(advIgnoreListMonitor));
            _bleDeviceManager = bleDeviceManager ?? throw new ArgumentNullException(nameof(bleDeviceManager));
            _workstationUnlocker = workstationUnlocker ?? throw new ArgumentNullException(nameof(workstationUnlocker));
            _screenActivator = screenActivator;

            _bleConnectionManager.AdvertismentReceived += BleConnectionManager_AdvertismentReceived;
            _proximitySettingsManager.SettingsChanged += UnlockerSettingsManager_SettingsChanged;

            SetAccessListFromSettings(_proximitySettingsManager.Settings);
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

        // Todo: Maybe add Start/Stop methods to TapConnectionProcessor

        void SetAccessListFromSettings(ProximitySettings settings)
        {
            MacListToConnect = settings.DevicesProximity.Select(s => s.Mac).ToList();
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
            if (adv == null)
                return;

            if (!_workstationUnlocker.IsConnected)
                return;

            if (Interlocked.CompareExchange(ref _isConnecting, 1, 1) == 1)
                return;

            if (MacListToConnect.Count == 0)
                return;

            var mac = MacUtils.GetMacFromShortMac(adv.Id);
            if (!MacListToConnect.Any(m => m == mac))
                return;

            var proximity = BleUtils.RssiToProximity(adv.Rssi);
            var settings = _proximitySettingsManager.Settings.GetProximitySettings(mac);
            if (proximity < settings.UnlockProximity)
                return;

            if (_advIgnoreListMonitor.IsIgnored(mac))
                return;

            if (_bleDeviceManager.Devices.Any(d => d.Mac == mac && d.IsConnected))
                return;

            if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0)
            { 
                try
                {
                    _screenActivator?.ActivateScreen();
                    await _connectionFlowProcessor.ConnectAndUnlock(mac);
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                    await _clientUiManager.SendNotification("");
                    await _clientUiManager.SendError(ex.Message);
                    _advIgnoreListMonitor.Ignore(mac);
                }
                finally
                {
                    Interlocked.Exchange(ref _isConnecting, 0);
                }
            }
        }
    }
}
