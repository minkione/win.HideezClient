using GalaSoft.MvvmLight.Messaging;
using HideezClient.HideezServiceReference;
using HideezClient.Messages;

namespace HideezClient.Modules.ProximityLockManager
{
    class ProximityLockManager: IProximityLockManager
    {
        private readonly ITaskbarIconManager _taskbarIconManager;
        private readonly IMessenger _messenger;

        public ProximityLockManager(ITaskbarIconManager taskbarIconManager, IMessenger messenger)
        {
            _taskbarIconManager = taskbarIconManager;
            _messenger = messenger;

            _messenger.Register<UnlockWorkstationMessage>(this, OnWorkstationUnlocked);
            _messenger.Register<DevicesCollectionChangedMessage>(this, OnDevicesCollectionChanged);
        }

        void OnWorkstationUnlocked(UnlockWorkstationMessage obj)
        {
            if (obj.IsDisabledLock)
                _messenger.Send(new ShowWarningNotificationMessage(message: "Lock by proximity is disabled"));

            ChangeIconState(obj.IsDisabledLock);
        }

        void OnDevicesCollectionChanged(DevicesCollectionChangedMessage obj)
        {
            foreach(DeviceDTO device in obj.Devices)
            {
                if(device.CanLockPyProximity)
                {
                    ChangeIconState(false);
                    return;
                }
            }
        }

        void ChangeIconState(bool isDisabledLock)
        {
            if(isDisabledLock)
                _taskbarIconManager.IconState = IconState.IdleAlert;
            else _taskbarIconManager.IconState = IconState.Idle;
        }
    }
}
