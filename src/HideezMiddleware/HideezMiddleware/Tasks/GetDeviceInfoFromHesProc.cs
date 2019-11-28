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
        readonly TaskCompletionSource<DeviceInfoDto> _tcs = new TaskCompletionSource<DeviceInfoDto>();

        public GetDeviceInfoFromHesProc(HesAppConnection hesConnection, string mac, CancellationToken ct)
        {
            _hesConnection = hesConnection;
            _mac = mac;
            _ct = ct;
        }

        internal Task<DeviceInfoDto> Run()
        {
            Task.Run(async () => 
            {
                try
                {
                    DeviceInfoDto info = null;

                    if (_hesConnection.State == HesConnectionState.Connected)
                    {
                        info = await _hesConnection.GetInfoByMac(_mac, _ct);
                    }
                    else
                    {
                        //todo - load device info from the local cache (file)
                        info = new DeviceInfoDto()
                        {
                            NeedUpdate = false
                        };
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
