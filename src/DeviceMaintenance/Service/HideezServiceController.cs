using DeviceMaintenance.Messages;
using Hideez.SDK.Communication.Log;
using Meta.Lib.Modules.PubSub;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;

namespace DeviceMaintenance.Service
{
    public class HideezServiceController
    {
        const string SERVICE_NAME = "Hideez Service";

        readonly EventLogger _log;
        readonly MetaPubSub _hub;
        readonly Timer _serviceStateRefreshTimer;
        readonly bool _restartServiceOnExit = false;

        ServiceController _serviceController;

        public bool IsServiceRunning => _serviceController?.Status == ServiceControllerStatus.Running;

        public HideezServiceController(EventLogger log, MetaPubSub hub)
        {
            try
            {
                _log = log;
                _hub = hub;

                _hub.Subscribe<ClosingEvent>(OnClosing);

                _serviceStateRefreshTimer = new Timer(2000);
                _serviceStateRefreshTimer.Elapsed += ServiceStateCheckTimer_Elapsed;
                _serviceStateRefreshTimer.AutoReset = true;
                _serviceStateRefreshTimer.Start();

                var controller = new ServiceController(SERVICE_NAME); // Will trigger ArgumentException if service is not installed
                var st = controller.Status; // Will trigger InvalidOperationException if service is not installed
                _serviceController = controller;

                if (IsServiceRunning)
                {
                    StopService();
                    _restartServiceOnExit = true;
                }
            }
            catch (InvalidOperationException)
            {
                // The most probable reason is that service is not installed. It is ok.
            }
            catch (ArgumentException)
            {
                // The most probable reason is that service is not installed. It is ok.
            }
        }

        private Task OnClosing(ClosingEvent arg)
        {
            if (_restartServiceOnExit && !IsServiceRunning)
                StartService();
            return Task.CompletedTask;
        }

        public void StopService()
        {
            try
            {
                _serviceController?.Refresh();

                if (_serviceController.CanStop)
                {
                    _serviceController?.Stop();
                    _serviceController?.Refresh();
                }
            }
            catch (Exception)
            {
                // todo: log exception
            }
        }

        public void StartService()
        {
            try
            {
                _serviceController?.Start();
            }
            catch (Exception)
            {
                // todo: log exception
            }
        }

        void ServiceStateCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_serviceController == null)
                {
                    var controller = new ServiceController(SERVICE_NAME);
                    var st = controller.Status; // Will trigger InvalidOperationException if service is not installed
                    _serviceController = controller;
                }

                _serviceController?.Refresh();
            }
            catch (InvalidOperationException)
            {
                // The most probable reason is that service is not installed. It is ok.
            }
            catch (ArgumentException)
            {
                // The most probable reason is that service is not installed. It is ok.
            }
            catch (Exception)
            {
                // todo: log exception
            }
        }
    }
}
