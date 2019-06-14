using NLog;
using System;
using System.ServiceModel;
using System.Linq;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService : IHideezService
    {
        static Logger _log;
        static bool _initialized = false;
        static object _initializationLock = new object();
        static ServiceClientSessionManager SessionManager = new ServiceClientSessionManager();

        ServiceClientSession _client;

        public HideezService()
        {
            lock (_initializationLock)
            {
                if (!_initialized)
                {
                    Initialize();
                    _initialized = true;
                }
            }
        }

        void Initialize()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            try
            {
                LogManager.EnableLogging();

                _log = LogManager.GetCurrentClassLogger();
                _log.Info(">>>>>> Starting service");

                _log.Info("CLR Version: {0}", Environment.Version);
                _log.Info("OS: {0}", Environment.OSVersion);
                _log.Info("Command: {0}", Environment.CommandLine);

                _log.Info(">>>>>> Initialize SDK");
                InitializeSDK();
                _log.Info(">>>>>> SDK Initialized");

                _log.Info(">>>>>> Service started");
            }
            catch (Exception ex)
            {
                _log.Error("Hideez Service has encountered an error during initialization." +
                    Environment.NewLine +
                    "The error must be resolved until service operation can be resumed. " +
                    Environment.NewLine +
                    "The service will not restart automatically.");
                _log.Error(ex);

                // Exit code 0 prevents automatic service restart trigger on exit
                Environment.Exit(0);
            }
        }

        #region Utils
        void ThrowException(Exception ex)
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

        void LogException(Exception ex)
        {
            _log.Error(ex.StackTrace);
            _log.Error(ex);
        }
        #endregion

        void Channel_Faulted(object sender, EventArgs e)
        {
            _log.Debug(">>>>>> Channel_Faulted");
            DetachClient();
        }

        void Channel_Closed(object sender, EventArgs e)
        {
            _log.Debug(">>>>>> Channel_Closed");
            DetachClient();
        }

        public bool AttachClient(ServiceClientParameters prm)
        {
            _log.Debug(">>>>>> AttachClient " + prm.ClientType.ToString());

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
            _client = SessionManager.Add(prm.ClientType, callback);

            OperationContext.Current.Channel.Closed += Channel_Closed;
            OperationContext.Current.Channel.Faulted += Channel_Faulted;

            return true;
        }

        public void DetachClient()
        {
            _log.Debug($">>>>>> DetachClient {_client?.ClientType}");
            SessionManager.Remove(_client);
        }

        public int Ping()
        {
            return 0;
        }

        public void Shutdown()
        {
            _log.Debug(">>>>>> Shutdown service");
            // Todo: shutdown service in a clean way
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                LogManager.EnableLogging();

                var fatalLogger = _log ?? LogManager.GetCurrentClassLogger();

                fatalLogger.Fatal(e.ExceptionObject as Exception);
                LogManager.Flush();
            }
            catch (Exception)
            {
                try
                {
                    Environment.FailFast("An error occured while handling fatal error", e.ExceptionObject as Exception);
                }
                catch (Exception exc)
                {
                    Environment.FailFast("An error occured while handling an error during fatal error handling", exc);
                }
            }
        }
    }
}
