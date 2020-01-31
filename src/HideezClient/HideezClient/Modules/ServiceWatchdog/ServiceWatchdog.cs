using Hideez.SDK.Communication.Log;
using HideezClient.HideezServiceReference;
using HideezClient.Modules.Log;
using HideezClient.Modules.ServiceProxy;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace HideezClient.Modules.ServiceWatchdog
{
    /// <summary>
    /// Responsible for keeping ServiceProxy connection alive and 
    /// automatically reconnecting to service when able
    /// </summary>
    sealed class ServiceWatchdog : IServiceWatchdog
    {
        private const int DELAY_BEFORE_START = 5000;
        private const int DELAY_AFTER_JOB = 3000;

        private readonly Logger log = LogManager.GetCurrentClassLogger(nameof(ServiceWatchdog));
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

                log.WriteLine("Watchdog started");

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
                log.WriteLine(ex);
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
                    await serviceProxy.ConnectAsync();
                }
                catch (FaultException<HideezServiceFault> ex)
                {
                    log.WriteLine(ex.FormattedMessage(), LogErrorSeverity.Error);
                    await serviceProxy.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    log.WriteLine(ex);
                    await serviceProxy.DisconnectAsync();
                }
            }
            else
            {
                try
                {
                    var ping = await serviceProxy.GetService().PingAsync();
                }
                catch (FaultException<HideezServiceFault> ex)
                {
                    log.WriteLine(ex.FormattedMessage(), LogErrorSeverity.Error);
                    await serviceProxy.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    log.WriteLine(ex);
                    await serviceProxy.DisconnectAsync();
                }
            }
        }
    }
}
