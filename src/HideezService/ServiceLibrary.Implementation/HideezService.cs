using System;
using System.ServiceModel;
using System.Linq;
using HideezMiddleware;
using Hideez.SDK.Communication;
using ServiceLibrary.Implementation.SessionManagement;
using Hideez.SDK.Communication.Log;
using System.Reflection;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService : IHideezService
    {
        static ILog _sdkLogger;
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

        void Initialize()
        {
            try
            {
                NLog.LogManager.EnableLogging();

                _sdkLogger = new NLogWrapper();
                _log = new Logger(nameof(HideezService), _sdkLogger);

                _log.WriteLine($">>>>>> Starting service: {Assembly.GetEntryAssembly().GetName().Version}");

                _log.WriteLine($"CLR Version: {Environment.Version}");
                _log.WriteLine($"OS: {Environment.OSVersion}");
                _log.WriteLine($"Command: {Environment.CommandLine}");


                _log.WriteLine(">>>>>> Initialize SDK");
                InitializeSDK();
                _log.WriteLine(">>>>>> SDK Initialized");

                _log.WriteLine(">>>>>> Service started");
            }
            catch (Exception ex)
            {
                _log.WriteLine("Hideez Service has encountered an error during initialization." +
                    Environment.NewLine +
                    "The error must be resolved until service operation can be resumed. " +
                    Environment.NewLine +
                    "The service will not restart automatically.", ex, LogErrorSeverity.Fatal);

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
                        new HideezServiceFault(baseEx.Message, (int)HideezErrorCode.NonHideezException), baseEx.Message);
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
                        new HideezServiceFault(ex.Message, (int)HideezErrorCode.NonHideezException), ex.Message);
                }
            }
        }

        public static void Error(Exception ex, string message = "")
        {
            _log.WriteLine(message, ex);
        }

        public static void Error(string message)
        {
            _log.WriteLine(message, LogErrorSeverity.Error);
        }
        #endregion

        void Channel_Faulted(object sender, EventArgs e)
        {
            _log.WriteLine(">>>>>> Channel_Faulted", LogErrorSeverity.Debug);
            DetachClient();
        }

        void Channel_Closed(object sender, EventArgs e)
        {
            _log.WriteLine(">>>>>> Channel_Closed", LogErrorSeverity.Debug);
            DetachClient();
        }

        public bool AttachClient(ServiceClientParameters prm)
        {
            _log.WriteLine(">>>>>> AttachClient " + prm.ClientType.ToString(), LogErrorSeverity.Debug);

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
            _log.WriteLine($">>>>>> DetachClient {_client?.ClientType}", LogErrorSeverity.Debug);
            SessionManager.Remove(_client);
            SessionManager.SessionClosed -= SessionManager_SessionClosed;
        }

        public int Ping()
        {
            return 0;
        }

        public void Shutdown()
        {
            _log.WriteLine(">>>>>> Shutdown service", LogErrorSeverity.Debug);
            // Todo: shutdown service in a clean way
        }
    }
}
