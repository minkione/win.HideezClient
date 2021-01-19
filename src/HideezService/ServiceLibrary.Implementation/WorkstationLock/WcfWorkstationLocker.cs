using Hideez.SDK.Communication.Log;
using HideezMiddleware;
using HideezMiddleware.IPC.Messages;
using HideezMiddleware.Utils.WorkstationHelper;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation.WorkstationLock
{
    class WcfWorkstationLocker : Logger, IWorkstationLocker
    {
        readonly IMetaPubSub _messenger;
        readonly IWorkstationHelper _workstationHelper;

        public WcfWorkstationLocker(IMetaPubSub messenger, IWorkstationHelper workstationHelper, ILog log)
            : base(nameof(WcfWorkstationLocker), log)
        {
            _messenger = messenger;
            _workstationHelper = workstationHelper;
        }

        public void LockWorkstation()
        {
            LockWorkstationAsync();
        }

        void LockWorkstationAsync()
        {
            Task.Run(async () =>
            {
                var lockState = _workstationHelper.GetActiveSessionLockState();
                if (lockState == WorkstationInformationHelper.LockState.Unlocked)
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
