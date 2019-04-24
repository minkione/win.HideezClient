using NLog;
using System;
using System.ServiceModel;
using System.Timers;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService : IHideezService
    {
        static Logger log;

        static bool initialized = false;
        static object initializationLock = new object();

        static ServiceClientSessionManager SessionManager = new ServiceClientSessionManager(); 

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

        //private void ThrowException(string message, HideezErrorCode code)
        //{
        //    throw new FaultException<HideezServiceFault>(
        //        new HideezServiceFault(message, (int)code), message);
        //}

        private void WriteLine(Exception ex)
        {
            //HideezCore.WriteLine(name, ex, LogErrorSeverity.Error);
        }

        private void WriteDebugLine(Exception ex)
        {
            //HideezCore.WriteDebugLine(name, ex, LogErrorSeverity.Error);
        }

        private void WriteDebugLine(string line)
        {
            //HideezCore.WriteDebugLine(name, line, LogErrorSeverity.Information);
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

            OperationContext.Current.Channel.Closed += Channel_Closed;
            OperationContext.Current.Channel.Faulted += Channel_Faulted;

            return true;
        }

        public void DetachClient()
        {
            log.Debug(">>>>>> DetachClient ");
        }

        public int Ping()
        {
            return 0;
        }

        public void Shutdown()
        {
            log.Debug(">>>>>> Shutdown service");
        }

        public bool GetAdapterState(Addapter addapter)
        {
            return true;
        }
    }
}
