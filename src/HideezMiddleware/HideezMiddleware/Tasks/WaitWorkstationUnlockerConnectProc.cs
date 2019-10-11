using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.Tasks
{
    internal class WaitWorkstationUnlockerConnectProc
    {
        readonly IWorkstationUnlocker _unlocker;
        readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public WaitWorkstationUnlockerConnectProc(IWorkstationUnlocker unlocker)
        {
            _unlocker = unlocker;
        }

        void Unlocker_Connected(object sender, EventArgs e)
        {
            _tcs.TrySetResult(true);
        }

        public async Task<bool> Run(int timeout, CancellationToken ct)
        {
            try
            {
                _unlocker.Connected += Unlocker_Connected;

                if (_unlocker.IsConnected)
                    return true;
                else
                    return await _tcs.Task.TimeoutAfter(timeout, ct);
            }
            catch (TimeoutException)
            {
            }
            finally
            {
                _unlocker.Connected -= Unlocker_Connected;
            }

            return false;
        }
    }
}
