using System;
using System.Threading;
using System.Threading.Tasks;
using Hideez.SDK.Communication.BLE;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Utils;

namespace HideezMiddleware.DeviceConnection
{
    public class TapConnectionProcessor : Logger, IDisposable
    {
        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly IBleConnectionManager _bleConnectionManager;
        readonly IScreenActivator _screenActivator;
        readonly IClientUiManager _clientUiManager;

        int _isConnecting = 0;

        public TapConnectionProcessor(
            ConnectionFlowProcessor connectionFlowProcessor,
            IBleConnectionManager bleConnectionManager,
            IScreenActivator screenActivator,
            IClientUiManager clientUiManager,
            ILog log) 
            : base(nameof(TapConnectionProcessor), log)
        {
            _connectionFlowProcessor = connectionFlowProcessor ?? throw new ArgumentNullException(nameof(connectionFlowProcessor));
            _bleConnectionManager = bleConnectionManager ?? throw new ArgumentNullException(nameof(bleConnectionManager));
            _clientUiManager = clientUiManager ?? throw new ArgumentNullException(nameof(clientUiManager));
            _screenActivator = screenActivator;

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
                _bleConnectionManager.AdvertismentReceived += BleConnectionManager_AdvertismentReceived;
            }

            disposed = true;
        }
        #endregion

        // Todo: Maybe add Start/Stop methods to TapConnectionProcessor

        async void BleConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            await UnlockByTap(e);
        }

        async Task UnlockByTap(AdvertismentReceivedEventArgs adv)
        {
            if (adv == null)
                return;

            if (adv.Rssi > -27)
            {
                if (Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0)
                {
                    try
                    {
                        _screenActivator?.ActivateScreen();
                        var mac = MacUtils.GetMacFromShortMac(adv.Id);
                        await _connectionFlowProcessor.ConnectAndUnlock(mac);
                    }
                    catch (Exception ex)
                    {
                        WriteLine(ex);
                        await _clientUiManager.SendNotification("");
                        await _clientUiManager.SendError(ex.Message);
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _isConnecting, 0);
                    }
                }
            }
        }
    }
}
