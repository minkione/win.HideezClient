using Hideez.SDK.Communication.Interfaces;
using System;

namespace HideezMiddleware
{
    public class WorkstationLockingEventArgs : EventArgs
    {
        public IDevice Device { get; private set; }

        public WorkstationLockingReason Reason { get; private set; }

        public WorkstationLockingEventArgs(IDevice device, WorkstationLockingReason reason)
        {
            Device = device;
            Reason = reason;
        }
    }
}
