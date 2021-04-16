using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Connection;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.ConnectionFlow
{
    public abstract class ConnectionFlowProcessorBase : Logger
    {
        int _workflowInterlock = 0;
        CancellationTokenSource _cts;

        public abstract event EventHandler<string> Started;
        public abstract event EventHandler<string> AttemptingUnlock;
        public abstract event EventHandler<string> UnlockAttempted;
        public abstract event EventHandler<IDevice> DeviceFinilizingMainFlow;
        public abstract event EventHandler<IDevice> DeviceFinishedMainFlow;
        public abstract event EventHandler<string> Finished;

        public bool IsRunning { get; protected set; }

        public ConnectionFlowProcessorBase(string name, ILog log) : base(name, log)
        {
            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;
        }

        void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            // Cancel the workflow if session switches to an unlocked (or different one)
            // Keep in mind, that workflow can cancel itself due to successful workstation unlock
            Cancel("Session switched");
        }

        protected void OnVaultDisconnectedDuringFlow(object sender, EventArgs e)
        {
            // Cancel the workflow if the vault disconnects
            Cancel("Vault unexpectedly disconnected");
        }

        protected void OnCancelledByVaultButton(object sender, EventArgs e)
        {
            // Cancel the workflow if the user pressed the cancel button (long button press)
            Cancel("User pressed the cancel button");
        }

        public void Cancel(string reason)
        {
            if (_cts != null)
            {
                WriteLine($"Canceling; {reason}");
                _cts?.Cancel();
            }
        }

        public async Task Connect(ConnectionId connectionId)
        {
            // ignore, if already performing workflow for any device
            if (Interlocked.CompareExchange(ref _workflowInterlock, 1, 0) == 0)
            {
                try
                {
                    _cts = new CancellationTokenSource();
                    await MainWorkflow(connectionId, false, false, null, _cts.Token);
                }
                finally
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;

                    Interlocked.Exchange(ref _workflowInterlock, 0);
                }
            }

        }

        public async Task ConnectAndUnlock(ConnectionId connectionId, Action<WorkstationUnlockResult> onSuccessfulUnlock)
        {
            // ignore, if already performing workflow for any device
            if (Interlocked.CompareExchange(ref _workflowInterlock, 1, 0) == 0)
            {
                try
                {
                    _cts = new CancellationTokenSource();
                    await MainWorkflow(connectionId, connectionId.IdProvider == (byte)DefaultConnectionIdProvider.Csr, true, onSuccessfulUnlock, _cts.Token);
                }
                finally
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;

                    Interlocked.Exchange(ref _workflowInterlock, 0);
                }
            }
        }

        protected abstract Task MainWorkflow(ConnectionId connectionId, bool rebondOnConnectionFail, bool tryUnlock, Action<WorkstationUnlockResult> onUnlockAttempt, CancellationToken ct);
    }
}
