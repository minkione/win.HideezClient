using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Settings;
using HideezMiddleware.Utils;

namespace HideezMiddleware.DeviceConnection
{

    public class ProximityConnectionProcessor : BaseConnectionProcessor, IDisposable
    {
        struct ProximityUnlockAccess
        {
            public string Mac { get; set; }
            public bool CanConnect { get; set; }
        }

        readonly IBleConnectionManager _bleConnectionManager;
        readonly IScreenActivator _screenActivator;
        readonly IClientUiManager _clientUiManager;
        readonly ISettingsManager<UnlockerSettings> _unlockerSettingsManager;
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
            ISettingsManager<UnlockerSettings> unlockerSettingsManager,
            AdvertisementIgnoreList advIgnoreListMonitor,
            BleDeviceManager bleDeviceManager,
            IWorkstationUnlocker workstationUnlocker,
            ILog log) 
            : base(connectionFlowProcessor, nameof(ProximityConnectionProcessor), log)
        {
            _bleConnectionManager = bleConnectionManager;
            _screenActivator = screenActivator;
            _clientUiManager = clientUi;
            _unlockerSettingsManager = unlockerSettingsManager;
            _advIgnoreListMonitor = advIgnoreListMonitor;
            _bleDeviceManager = bleDeviceManager;
            _workstationUnlocker = workstationUnlocker;
            
            _bleConnectionManager.AdvertismentReceived += BleConnectionManager_AdvertismentReceived;
            _unlockerSettingsManager.SettingsChanged += UnlockerSettingsManager_SettingsChanged;

            SetAccessListFromSettings(_unlockerSettingsManager.Settings);
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
                _unlockerSettingsManager.SettingsChanged -= UnlockerSettingsManager_SettingsChanged;
            }

            disposed = true;
        }

        ~ProximityConnectionProcessor()
        {
            Dispose(false);
        }
        #endregion

        // Todo: Maybe add Start/Stop methods to TapConnectionProcessor

        void SetAccessListFromSettings(UnlockerSettings settings)
        {
            MacListToConnect = settings.DeviceUnlockerSettings
                .Where(s => s.AllowProximity)
                .Select(s => s.Mac)
                .ToList();
        }

        void UnlockerSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<UnlockerSettings> e)
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
            if (proximity < _unlockerSettingsManager.Settings.UnlockProximity)
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
                    await ConnectDeviceByMac(mac);
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
