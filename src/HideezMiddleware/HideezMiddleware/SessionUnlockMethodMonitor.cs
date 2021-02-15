using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceConnection.Workflow.ConnectionFlow;
using HideezMiddleware.Tasks;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware
{
    public class SessionUnlockMethodMonitor : Logger
    {
        readonly ConnectionFlowProcessorBase _connectionFlowProcessor;
        private readonly IMetaPubSub _messenger;

        UnlockSessionSwitchProc _unlockProcedure = null;
        readonly object _upLock = new object();

        internal UnlockSessionSwitchProc UnlockProcedure { get => _unlockProcedure; }

        public SessionUnlockMethodMonitor(ConnectionFlowProcessorBase connectionFlowProcessor,
            IMetaPubSub messenger,
            ILog log)
            : base(nameof(SessionUnlockMethodMonitor), log)
        {
            _connectionFlowProcessor = connectionFlowProcessor;
            _messenger = messenger;

            _connectionFlowProcessor.Started += ConnectionFlowProcessor_Started;
        }

        void ConnectionFlowProcessor_Started(object sender, string e)
        {
            lock (_upLock)
            {
                if (_unlockProcedure != null)
                    _unlockProcedure.Dispose();
                
                _unlockProcedure = new UnlockSessionSwitchProc(e, _connectionFlowProcessor, _messenger);
                WriteLine("Started unlock procedure");
            }
        }

        public SessionSwitchSubject GetUnlockMethod()
        {
            if (_unlockProcedure == null)
                return SessionSwitchSubject.NonHideez;
            else 
                return _unlockProcedure.UnlockMethod;
        }
    }
}
