using NLog;
using System;
using System.ServiceModel;
using System.Linq;
using HideezMiddleware;
using Hideez.SDK.Communication;
using System.Threading.Tasks;

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
                    SetupExceptionHandling();
                    Initialize();
                    _initialized = true;
                }
            }
        }

        void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
        }

        void LogUnhandledException(Exception e, string source)
        {
            try
            {
                LogManager.EnableLogging();

                var fatalLogger = _log ?? LogManager.GetCurrentClassLogger();
                var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();

                fatalLogger.Fatal($"Unhandled exception in {assemblyName.Name} v{assemblyName.Version}");
                fatalLogger.Fatal(e);
                LogManager.Flush();
                LogManager.Shutdown();
            }
            catch (Exception)
            {
                try
                {
                    Environment.FailFast("An error occured while handling fatal error", e as Exception);
                }
                catch (Exception exc)
                {
                    Environment.FailFast("An error occured while handling an error during fatal error handling", exc);
                }
            }
        }

        void Initialize()
        {
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
                _log.Fatal("Hideez Service has encountered an error during initialization." +
                    Environment.NewLine +
                    "The error must be resolved until service operation can be resumed. " +
                    Environment.NewLine +
                    "The service will not restart automatically.");
                _log.Fatal(ex);

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
                if (baseEx is HideezException hideezEx)
                {
                    throw new FaultException<HideezServiceFault>(
                        new HideezServiceFault(HideezExceptionLocalization.GetErrorAsString(hideezEx), (int)hideezEx.ErrorCode), hideezEx.Message);
                }
                else
                {
                    throw new FaultException<HideezServiceFault>(
                        new HideezServiceFault(baseEx.Message, (int)HideezErrorCode.GenericException), baseEx.Message);
                }
            }
            else
            {
                if (ex is HideezException hideezEx)
                {
                    throw new FaultException<HideezServiceFault>(
                        new HideezServiceFault(HideezExceptionLocalization.GetErrorAsString(hideezEx), (int)hideezEx.ErrorCode), hideezEx.Message);
                }
                else
                {
                    throw new FaultException<HideezServiceFault>(
                        new HideezServiceFault(ex.Message, (int)HideezErrorCode.GenericException), ex.Message);
                }
            }
        }

        public static void LogException(Exception ex)
        {
            _log.Error(ex);
            _log.Error(ex.StackTrace);
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
            SessionManager.SessionClosed += SessionManager_SessionClosed;

            return true;
        }

        public void DetachClient()
        {
            _log.Debug($">>>>>> DetachClient {_client?.ClientType}");
            SessionManager.Remove(_client);
            SessionManager.SessionClosed -= SessionManager_SessionClosed;
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
    }
}
