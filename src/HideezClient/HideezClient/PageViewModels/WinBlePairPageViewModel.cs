using HideezClient.Mvvm;
using HideezClient.ViewModels.Controls;

namespace HideezClient.PageViewModels
{
    class WinBlePairPageViewModel : LocalizedObject
    {
        public WinBleDeviceManagementListViewModel DeviceManagementListVm { get; }
        
        public WinBlePairPageViewModel(WinBleDeviceManagementListViewModel deviceManagementListVm)
        {
            DeviceManagementListVm = deviceManagementListVm;
        }
    }
}
