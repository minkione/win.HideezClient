using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using HideezMiddleware.Localize;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;
using HideezMiddleware.ApplicationModeProvider;

namespace HideezClient.Modules.ProximityLockManager
{
    class ProximityLockManager: IProximityLockManager
    {
        private readonly ITaskbarIconManager _taskbarIconManager;
        readonly IMetaPubSub _metaMessenger;
        private readonly IApplicationModeProvider _applicationModeProvider;

        public ProximityLockManager(ITaskbarIconManager taskbarIconManager, IMetaPubSub metaMessenger, IApplicationModeProvider applicationModeProvider)
        {
            _taskbarIconManager = taskbarIconManager;
            _metaMessenger = metaMessenger;
            _applicationModeProvider = applicationModeProvider;

            _metaMessenger.TrySubscribeOnServer<WorkstationUnlockedMessage>(OnWorkstationUnlocked);
            _metaMessenger.TrySubscribeOnServer<HideezMiddleware.IPC.Messages.DevicesCollectionChangedMessage>(OnDevicesCollectionChanged);
        }

        async Task OnWorkstationUnlocked(WorkstationUnlockedMessage message)
        {
            if (_applicationModeProvider.GetApplicationMode() == ApplicationMode.Enterprise)
            {
                if (message.IsNotHideezMethod)
                    await _metaMessenger.Publish(new ShowWarningNotificationMessage(message: TranslationSource.Instance["Notification.ProximityLockDisabled"]));

                ChangeIconState(message.IsNotHideezMethod);
            }
        }

        Task OnDevicesCollectionChanged(HideezMiddleware.IPC.Messages.DevicesCollectionChangedMessage obj)
        {
            foreach(HideezMiddleware.IPC.DTO.DeviceDTO device in obj.Devices)
            {
                if(device.CanLockPyProximity)
                {
                    ChangeIconState(false);
                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }

        void ChangeIconState(bool isDisabledLock)
        {
            if(isDisabledLock)
                _taskbarIconManager.IconState = IconState.IdleAlert;
            else _taskbarIconManager.IconState = IconState.Idle;
        }
    }
}
