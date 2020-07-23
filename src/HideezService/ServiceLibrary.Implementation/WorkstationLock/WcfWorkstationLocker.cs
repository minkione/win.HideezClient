using Hideez.SDK.Communication.Log;
using HideezMiddleware;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation.WorkstationLock
{
    class WcfWorkstationLocker : Logger, IWorkstationLocker
    {
        readonly IMetaPubSub _messenger;

        public WcfWorkstationLocker(IMetaPubSub messenger, ILog log)
            : base(nameof(WcfWorkstationLocker), log)
        {
            _messenger = messenger;
        }

        public void LockWorkstation()
        {
            LockWorkstationAsync();
        }

        void LockWorkstationAsync()
        {
            Task.Run(async () =>
            {
                var lockState = WorkstationHelper.GetActiveSessionLockState();
                if (lockState == WorkstationHelper.LockState.Unlocked)
                {
                    try
                    {
                        await _messenger.Publish(new LockWorkstationMessage());
                    }
                    catch (Exception ex)
                    {
                        WriteLine(ex);
                    }
                }
            });
        }
    }
}
