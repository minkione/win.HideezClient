using System;
using HideezMiddleware;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using System.Reflection;
using System.Threading.Tasks;
using HideezMiddleware.Audit;
using Microsoft.Win32;
using HideezMiddleware.Workstation;
using Meta.Lib.Modules.PubSub;
using System.IO.Pipes;
using System.Security.Principal;
using System.Security.AccessControl;
using HideezMiddleware.Utils.WorkstationHelper;
using HideezMiddleware.ConnectionModeProvider;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService
    {
        static ILog _sdkLogger;
        static Logger _log;
        static EventSaver _eventSaver;

        static bool _initialized = false;
        static object _initializationLock = new object();
        static SessionInfoProvider _sessionInfoProvider;
        static SessionTimestampLogger _sessionTimestampLogger;
        static RegistryKey clientRootRegistryKey;
        static IWorkstationIdProvider _workstationIdProvider;
        static IMetaPubSub _messenger;
        static IWorkstationHelper _workstationHelper;
        static IConnectionModeProvider _connectionModeProvider;

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

                _log.WriteLine($">>>>>> Starting service");

                _log.WriteLine($"Service Version: {Assembly.GetEntryAssembly().GetName().Version}");
                _log.WriteLine($"CLR Version: {Environment.Version}");
                _log.WriteLine($"OS: {Environment.OSVersion}");
                _log.WriteLine($"Command: {Environment.CommandLine}");

                _log.WriteLine($">>>>> Starting messaging hub");
                var pubSubLogger = new MetaPubSubLogger(_sdkLogger);
                _messenger = new MetaPubSub(pubSubLogger);

                _log.WriteLine(">>>>>> Get registry settings key");
                clientRootRegistryKey = HideezClientRegistryRoot.GetRootRegistryKey(true);

                _log.WriteLine(">>>>>> Get connection mode");
                _connectionModeProvider = new ConnectionModeProvider(clientRootRegistryKey, _sdkLogger);
                _log.WriteLine($"Connection mode: {_connectionModeProvider.ConnectionMode}");

                _log.WriteLine(">>>>>> Initialize session monitor");
                _workstationHelper = new WorkstationHelper(_sdkLogger);
                _sessionInfoProvider = new SessionInfoProvider(_workstationHelper, _sdkLogger);

                _log.WriteLine(">>>>>> Initialize workstation id provider");
                _workstationIdProvider = new WorkstationIdProvider(clientRootRegistryKey, _sdkLogger);
                if (string.IsNullOrWhiteSpace(_workstationIdProvider.GetWorkstationId()))
                    _workstationIdProvider.SaveWorkstationId(Guid.NewGuid().ToString());

                _log.WriteLine(">>>>>> Initilize audit");
                var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                string auditEventsDirectoryPath = $@"{commonAppData}\Hideez\Service\WorkstationEvents\";
                _eventSaver = new EventSaver(_sessionInfoProvider, _workstationIdProvider, auditEventsDirectoryPath, _sdkLogger);

                _log.WriteLine(">>>>>> Initialize session timestamp monitor");
                var sessionTimestampPath = $@"{commonAppData}\Hideez\Service\Timestamp\timestamp.dat";
                _sessionTimestampLogger = new SessionTimestampLogger(sessionTimestampPath, _sessionInfoProvider, _eventSaver, _workstationHelper, _sdkLogger);

                OnServiceStarted();

                _log.WriteLine(">>>>>> Initialize SDK");
                InitializeSDK().Wait();

                _messenger.StartServer("HideezServicePipe", () =>
                {
                    try
                    {
                        _log.WriteLine("Custom pipe config started");
                        var pipeSecurity = new PipeSecurity();
                        pipeSecurity.AddAccessRule(new PipeAccessRule(
                            new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                            PipeAccessRights.FullControl,
                            AccessControlType.Allow));
                
                        var pipe = new NamedPipeServerStream("HideezServicePipe", PipeDirection.InOut, 32,
                            PipeTransmissionMode.Message, PipeOptions.Asynchronous, 4096, 4096, pipeSecurity);

                        _log.WriteLine("Custom pipe config successful");
                        return pipe;
                    }
                    catch (Exception ex)
                    {
                        _log.WriteLine("Custom pipe config failed.", ex);
                        return null;
                    }
                });

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
        void Error(Exception ex, string message = "")
        {
            _log?.WriteLine(message, ex);
        }

        void Error(string message)
        {
            _log?.WriteLine(message, LogErrorSeverity.Error);
        }

        async Task SafePublish(IPubSubMessage message, bool logError = false)
        {
            try
            {
                await _messenger.Publish(message);
            }
            catch (Exception ex)
            {
                if (logError)
                    Error(ex);
            }
        }
        #endregion

        public void Shutdown()
        {
            _log.WriteLine(">>>>>> Shutdown service", LogErrorSeverity.Debug);
            OnServiceStopped();
            // Todo: shutdown service in a clean way
        }

        #region Host Only
        void OnServiceStarted()
        {
            // Generate event for audit
            var workstationEvent = _eventSaver.GetWorkstationEvent();
            workstationEvent.EventId = WorkstationEventType.ServiceStarted;
            Task.Run(() => _eventSaver.AddNewAsync(workstationEvent));
        }

        void OnServiceStopped()
        {
            try
            {
                // Generate event for audit
                var workstationEvent = _eventSaver.GetWorkstationEvent();
                workstationEvent.EventId = WorkstationEventType.ServiceStopped;
                _eventSaver.AddNew(workstationEvent); 
            }
            catch (Exception) { }
        }
        #endregion
    }
}
