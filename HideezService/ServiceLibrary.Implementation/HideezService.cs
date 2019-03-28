using NLog;
using ServiceLibrary;
using System;
using System.ServiceModel;

namespace ServiceLibrary.Implementation
{
    public class HideezService : IHideezService
    {
        ServiceClientSessionManager sessionManager;
        ServiceClientSession user;

        public HideezService()
        {
            LogManager.EnableLogging();

            sessionManager = new ServiceClientSessionManager();
            Log = LogManager.GetCurrentClassLogger();

            Log.Info(">>>>>> Service started");
            Log.Info("CLR Version: {0}", Environment.Version);
            Log.Info("OS: {0}", Environment.OSVersion);
            Log.Info("Command: {0}", Environment.CommandLine);
        }

        public static Logger Log { get; private set; }

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
            Log.Debug(">>>>>> Channel_Faulted");
            DetachClient();
        }

        private void Channel_Closed(object sender, EventArgs e)
        {
            Log.Debug(">>>>>> Channel_Closed");
            DetachClient();
        }

        public bool AttachClient(ServiceClientParameters prm)
        {
            Log.Debug(">>>>>> AttachClient " + prm.ClientType.ToString());

            OperationContext.Current.Channel.Closed += Channel_Closed;
            OperationContext.Current.Channel.Faulted += Channel_Faulted;

            return true;
        }

        public void DetachClient()
        {
            Log.Debug(">>>>>> DetachClient ");

            if (user != null)
            {
                sessionManager.Remove(user);
            }
        }

        public byte[] Ping(byte[] ping)
        {
            return ping;
        }

        public void Shutdown()
        {
            Log.Debug(">>>>>> Shutdown service");
        }
    }
}
