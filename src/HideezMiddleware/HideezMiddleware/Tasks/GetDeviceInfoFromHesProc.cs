using System;
using System.Threading;
using System.Threading.Tasks;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;

namespace HideezMiddleware.Tasks
{
    // fetching the device info from the HES in the background
    public class GetDeviceInfoFromHesProc
    {
        readonly HesAppConnection _hesConnection;
        readonly string _mac;
        readonly CancellationToken _ct;
        readonly TaskCompletionSource<HwVaultShortInfoFromHesDto> _tcs = new TaskCompletionSource<HwVaultShortInfoFromHesDto>();

        /// <summary>
        /// Set to True if info was retrieved from HES
        /// </summary>
        public bool IsSuccessful { get; private set; }

        public GetDeviceInfoFromHesProc(HesAppConnection hesConnection, string mac, CancellationToken ct)
        {
            _hesConnection = hesConnection;
            _mac = mac;
            _ct = ct;
        }

        internal Task<HwVaultShortInfoFromHesDto> Run()
        {
            Task.Run(async () => 
            {
                try
                {
                    HwVaultShortInfoFromHesDto info = null;

                    if (_hesConnection.State == HesConnectionState.Connected)
                    {
                        info = await _hesConnection.GetHwVaultInfoByMac(_mac, _ct);
                        IsSuccessful = true;
                    }
                    else
                    {
                        //todo - load device info from the local cache (file)
                        info = new HwVaultShortInfoFromHesDto();
                        IsSuccessful = false;

                    }

                    _tcs.TrySetResult(info);
                }
                catch (Exception ex)
                {
                    _tcs.TrySetException(ex);
                }
            });

            return _tcs.Task;
        }
    }
}
