using HideezSafe.Modules.ServiceProxy;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezSafe.Modules.ServiceWatchdog
{
    /// <summary>
    /// Responsible for keeping ServiceProxy connection alive and 
    /// automatically reconnecting to service when able
    /// </summary>
    sealed class ServiceWatchdog : IServiceWatchdog
    {
        private const int DELAY_BEFORE_START = 5000;
        private const int DELAY_AFTER_JOB = 3000;

        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IServiceProxy serviceProxy;

        private CancellationTokenSource cancel;

        public ServiceWatchdog(IServiceProxy serviceProxy)
        {
            this.serviceProxy = serviceProxy;
        }

        public void Start()
        {
            cancel = new CancellationTokenSource();

            Task.Run(async () =>
            {
                // a little delay before the watchdog starts to do his work
                await Task.Delay(DELAY_BEFORE_START, cancel.Token);

                log.Info("Watchdog started");

                WorkFunc();
            });
        }

        public void Stop()
        {
            cancel?.Cancel();
        }

        private async void WorkFunc()
        {
            try
            {
                if (cancel.IsCancellationRequested)
                    return;

                await CheckServerConnection();

                await Task.Delay(DELAY_AFTER_JOB, cancel.Token);
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                log.Error(ex);
                await Task.Delay(DELAY_AFTER_JOB, cancel.Token);
            }
            finally
            {
                if (!cancel.IsCancellationRequested)
                {
                    await Task.Run(() => WorkFunc());
                }
            }
        }

        private async Task CheckServerConnection()
        {
            if (!serviceProxy.IsConnected)
            {
                try
                {
                    var result = await serviceProxy.ConnectAsync();
                    if (!result)
                        await serviceProxy.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    await serviceProxy.DisconnectAsync();
                }
            }
            else
            {
                try
                {
                    var ping = await serviceProxy.GetService().PingAsync(new byte[2] { 1, 7 });
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    await serviceProxy.DisconnectAsync();
                }
            }
        }
    }
}
