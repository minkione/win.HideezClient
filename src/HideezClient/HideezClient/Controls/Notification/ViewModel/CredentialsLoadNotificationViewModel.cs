using HideezClient.Models;
using HideezClient.Mvvm;
using MvvmExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Controls
{
    public enum LoadedCredentialsState
    {
        Loading,
        Loaded,
        Cancel,
        Fail,
    }

    public class CredentialsLoadNotificationViewModel : ObservableObject
    {
        private DeviceModel device;
        private LoadedCredentialsState state;

        public CredentialsLoadNotificationViewModel(DeviceModel device)
        {
            this.Device = device;
            Device.PropertyChanged += Device_PropertyChanged;

        }

        private void Device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is DeviceModel device)
            {
                if (e.PropertyName == nameof(Device.IsStorageLoaded) && device.IsStorageLoaded)
                {
                    State = LoadedCredentialsState.Loaded;
                }
                else if (e.PropertyName == nameof(Device.IsLoadingStorage) && !device.IsLoadingStorage && !device.IsStorageLoaded)
                {
                    State = LoadedCredentialsState.Fail;
                }
            }
        }

        public DeviceModel Device
        {
            get { return device; }
            set { Set(ref device, value); }
        }

        [DependsOn(nameof(Device))]
        public string DeviceSN
        {
            get { return device.SerialNo; }
        }

        public LoadedCredentialsState State
        {
            get { return state; }
            set { Set(ref state, value); }
        }
    }
}
