using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;
using WinBle;

namespace HideezMiddleware.Tasks
{
    class WaitAdvertisementProc
    {
        readonly TaskCompletionSource<AdvertismentReceivedEventArgs> _tcs = new TaskCompletionSource<AdvertismentReceivedEventArgs>();
        readonly IBleConnectionManager _bleConnectionManager;

        public WaitAdvertisementProc(IBleConnectionManager connectionManager)
        {
            _bleConnectionManager = connectionManager;
        }

        public async Task<AdvertismentReceivedEventArgs> Run(int timeout)
        {
            try
            {
                _bleConnectionManager.AdvertismentReceived += WinBleConnectionManager_AdvertismentReceived;

                var res = await _tcs.Task.TimeoutAfter(timeout);

                return res;
            }
            catch (TimeoutException)
            {
                return null;
            }
            finally
            {
                _bleConnectionManager.AdvertismentReceived -= WinBleConnectionManager_AdvertismentReceived;
            }
        }

        private void WinBleConnectionManager_AdvertismentReceived(object sender, AdvertismentReceivedEventArgs e)
        {
            _tcs.TrySetResult(e);
        }
    }
}
