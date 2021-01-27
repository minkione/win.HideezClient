using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;
using WinBle._10._0._18362;

namespace HideezMiddleware.Tasks
{
    class WaitAdvertisementProc
    {
        readonly TaskCompletionSource<AdvertismentReceivedEventArgs> _tcs = new TaskCompletionSource<AdvertismentReceivedEventArgs>();
        readonly WinBleConnectionManager _winBleConnectionManager;

        public WaitAdvertisementProc(WinBleConnectionManager connectionManager)
        {
            _winBleConnectionManager = connectionManager;
        }

        public async Task<AdvertismentReceivedEventArgs> Run(int timeout)
        {
            try
            {
                _winBleConnectionManager.AdvertismentReceived += WinBleConnectionManager_AdvertismentReceived;

                var res = await _tcs.Task.TimeoutAfter(timeout);

                return res;
            }
            catch (TimeoutException)
            {
                return null;
            }
            finally
            {
                _winBleConnectionManager.AdvertismentReceived -= WinBleConnectionManager_AdvertismentReceived;
            }
        }

        private void WinBleConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            if (_winBleConnectionManager.BondedControllers.FirstOrDefault(c => c.Connection.ConnectionId.Id == e.Id) != null)
                _tcs.TrySetResult(e);
        }
    }
}
