using HideezClient.Mvvm;
using HideezMiddleware.IPC.DTO;
using HideezMiddleware.IPC.IncommingMessages;
using Meta.Lib.Modules.PubSub;
using MvvmExtensions.Attributes;
using MvvmExtensions.Commands;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace HideezClient.ViewModels.Controls
{
    public class WinBleControllerStateViewModel : LocalizedObject
    {
        readonly IMetaPubSub _metaMessenger;

        string _id;
        string _name;
        string _mac;
        bool _isConnected;
        bool _isDiscovered;

        public string Id
        {
            get { return _id; }
            set { Set(ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        public string Mac
        {
            get { return _mac; }
            set { Set(ref _mac, value); }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set { Set(ref _isConnected, value); }
        }

        public bool IsDiscovered
        {
            get { return _isDiscovered; }
            set { Set(ref _isDiscovered, value); }
        }

        [DependsOn(nameof(IsConnected), nameof(IsDiscovered))]
        public bool CanBeConnected
        {
            get { return !IsConnected && IsDiscovered; }
        }

        public ICommand ConnectCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        OnConnectCommand();
                    }
                };
            }
        }

        public WinBleControllerStateViewModel(IMetaPubSub metaMessenger)
        {
            _metaMessenger = metaMessenger;
        }

        public void FromDto(WinBleControllerStateDTO dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Mac = dto.Mac;
            IsConnected = dto.IsConnected;
            IsDiscovered = dto.IsDiscovered;
        }

        async void OnConnectCommand()
        {
            try
            {
                await _metaMessenger.PublishOnServer(new ConnectDeviceRequestMessage(Id));
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }
    }
}
