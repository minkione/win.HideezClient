﻿using HideezClient.Mvvm;
using HideezClient.ViewModels;
using Unity;

namespace HideezClient.PageViewModels
{
    class SettingsPageViewModel : LocalizedObject
    {
        [Dependency]
        public ServiceViewModel Service { get; set; }

        [Dependency]
        public SoftwareUnlockSettingViewModel SoftwareUnlock { get; set; }

        [Dependency]
        public ReconnectPairedVaultsControlViewModel PairedVaultsReconnect { get; set; }
    }
}
