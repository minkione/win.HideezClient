using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceConnection;
using HideezMiddleware.DeviceConnection.Workflow;
using HideezMiddleware.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class SessionUnlockMethodMonitor : Logger
    {
        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly TapConnectionProcessor _tapProcessor;
        readonly RfidConnectionProcessor _rfidProcessor;
        readonly ProximityConnectionProcessor _proximityProcessor;
        readonly ExternalConnectionProcessor _winBleProcessor;

        UnlockSessionSwitchProc _unlockProcedure = null;
        readonly object _upLock = new object();

        internal UnlockSessionSwitchProc UnlockProcedure { get => _unlockProcedure; }

        public SessionUnlockMethodMonitor(ConnectionFlowProcessor connectionFlowProcessor,
                                          TapConnectionProcessor tapProcessor,
                                          RfidConnectionProcessor rfidProcessor,
                                          ProximityConnectionProcessor proximityProcessor,
                                          ExternalConnectionProcessor winBleProcessor,
                                          ILog log): base(nameof(SessionUnlockMethodMonitor), log)
        {
            _connectionFlowProcessor = connectionFlowProcessor;
            _tapProcessor = tapProcessor;
            _rfidProcessor = rfidProcessor;
            _proximityProcessor = proximityProcessor;
            _winBleProcessor = winBleProcessor;

            _connectionFlowProcessor.Started += ConnectionFlowProcessor_Started;
        }

        void ConnectionFlowProcessor_Started(object sender, string e)
        {
            lock (_upLock)
            {
                if (_unlockProcedure != null)
                    _unlockProcedure.Dispose();

                _unlockProcedure = new UnlockSessionSwitchProc(e, _connectionFlowProcessor, _tapProcessor, _rfidProcessor, _proximityProcessor, _winBleProcessor);
                WriteLine("Started unlock procedure");
            }
        }

        public SessionSwitchSubject GetUnlockMethod()
        {
            if (_unlockProcedure == null)
                return SessionSwitchSubject.NonHideez;
            else return _unlockProcedure.UnlockMethod;
        }
    }
}
