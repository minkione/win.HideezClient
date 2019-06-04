using NLog;
using System;
using System.ServiceModel;
using System.Linq;
using System.Threading;
using Hideez.SDK.Communication.Interfaces;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService : IHideezService
    {
        static Logger log;

        static bool initialized = false;
        static object initializationLock = new object();

        static ServiceClientSessionManager SessionManager = new ServiceClientSessionManager();

        private ServiceClientSession client;

        public HideezService()
        {
            lock (initializationLock)
            {
                if (!initialized)
                {
                    Initialize();
                    initialized = true;
                }
            }
        }

        private void Initialize()
        {
            try
            {
                LogManager.EnableLogging();

                log = LogManager.GetCurrentClassLogger();
                log.Info(">>>>>> Starting service");

                log.Info("CLR Version: {0}", Environment.Version);
                log.Info("OS: {0}", Environment.OSVersion);
                log.Info("Command: {0}", Environment.CommandLine);

                log.Info(">>>>>> Initialize SDK");
                InitializeSDK();
                log.Info(">>>>>> SDK Initialized");

                log.Info(">>>>>> Service started");
            }
            catch (Exception ex)
            {
                log.Error("Hideez Service has encountered an error during initialization." +
                    Environment.NewLine +
                    "The error must be resolved until service operation can be resumed. " +
                    Environment.NewLine +
                    "The service will not restart automatically.");
                log.Error(ex);

                // Exit code 0 prevents automatic service restart trigger on exit
                Environment.Exit(0);
            }
        }

        #region Utils
        private void ThrowException(Exception ex)
        {
            if (ex is AggregateException agg)
            {
                var baseEx = agg.GetBaseException();

                throw new FaultException<HideezServiceFault>(
                    new HideezServiceFault(baseEx.Message, 6), baseEx.Message);
            }
            else
            {
                throw new FaultException<HideezServiceFault>(
                    new HideezServiceFault(ex.Message, 6), ex.Message);
            }
        }
        #endregion

        private void Channel_Faulted(object sender, EventArgs e)
        {
            log.Debug(">>>>>> Channel_Faulted");
            DetachClient();
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            log.Debug(">>>>>> Channel_Closed");
            DetachClient();
        }

        public bool AttachClient(ServiceClientParameters prm)
        {
            log.Debug(">>>>>> AttachClient " + prm.ClientType.ToString());

            // Limit to one ServiceHost / TestConsole connection
            if (prm.ClientType == ClientType.TestConsole ||
                prm.ClientType == ClientType.ServiceHost)
            {
                if (SessionManager.Sessions.Any(s =>
                s.ClientType == ClientType.ServiceHost ||
                s.ClientType == ClientType.TestConsole))
                {
                    throw new Exception("Service does not support more than one connected ServiceHost or TestConsole client");
                }
            }

            var callback = OperationContext.Current.GetCallbackChannel<ICallbacks>();
            client = SessionManager.Add(prm.ClientType, callback);

            OperationContext.Current.Channel.Closed += Channel_Closed;
            OperationContext.Current.Channel.Faulted += Channel_Faulted;

            return true;
        }

        public void DetachClient()
        {
            log.Debug(">>>>>> DetachClient ");
            SessionManager.Remove(client);
        }

        public int Ping()
        {
            return 0;
        }

        public void Shutdown()
        {
            log.Debug(">>>>>> Shutdown service");
            // Todo: shutdown service in a clean way
        }


        public void SaveCredential(string deviceId, string login, string password)
        {
            log.Debug($">>>>>> Save credential.");
            // Todo: Save credential
        }

        public void DisconnectDevice(string deviceId)
        {
            _deviceManager.Devices.FirstOrDefault(d => d.Id == deviceId)?.Connection.Disconnect();
        }

        public async void RemoveDevice(string deviceId)
        {
            var device = _deviceManager.Devices.FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
                await _deviceManager.Remove(device);
        }
    }
}
