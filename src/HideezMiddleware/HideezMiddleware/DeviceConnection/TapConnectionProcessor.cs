using System;
using System.Linq;
using System.Threading.Tasks;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Settings;
using HideezMiddleware.Utils;

namespace HideezMiddleware.DeviceConnection
{
    class TapConnectionProcessor : BlacklistConnectionProcessor, IDisposable
    {
        readonly IBleConnectionManager _bleConnectionManager;
        readonly IScreenActivator _screenActivator;
        readonly IClientUi _clientUi;
        readonly ISettingsManager<UnlockerSettings> _unlockerSettingsManager;

        public TapConnectionProcessor(
            ConnectionFlowProcessor connectionFlowProcessor,
            IBleConnectionManager bleConnectionManager,
            IScreenActivator screenActivator,
            IClientUi clientUi,
            ISettingsManager<UnlockerSettings> unlockerSettingsManager,
            ILog log) 
            : base(connectionFlowProcessor, nameof(TapConnectionProcessor), log)
        {
            _bleConnectionManager = bleConnectionManager;
            _screenActivator = screenActivator;
            _clientUi = clientUi;
            _unlockerSettingsManager = unlockerSettingsManager;

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
        #endregion

        // Todo: Maybe add Start/Stop methods to TapConnectionProcessor

        void SetAccessListFromSettings(UnlockerSettings settings)
        {
            AccessList = settings.DeviceUnlockerSettings
                .Where(s => s.AllowBleTap)
                .Select(s => new ShortDeviceInfo()
                {
                    Mac = s.Mac,
                    SerialNo = s.SerialNo
                })
                .ToList();
        }

        void UnlockerSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<UnlockerSettings> e)
        {
            SetAccessListFromSettings(e.NewSettings);
        }

        async void BleConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            await UnlockByTap(e);
        }

        async Task UnlockByTap(AdvertismentReceivedEventArgs adv)
        {
            try
            {
                if (adv.Rssi > -27)
                {
                    var mac = MacUtils.GetMacFromShortMac(adv.Id);
                    await ConnectDeviceByMac(mac);
                }
            }
            catch (AccessDeniedAuthException ex)
            {
                WriteLine(ex);
                await _clientUi.SendNotification("");
                await _clientUi.SendError(ex.Message);
            }
        }
    }
}
