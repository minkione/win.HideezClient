using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using NLog;
using System;
using System.Collections.ObjectModel;

namespace HideezClient.ViewModels
{
    class IndicatorsViewModel : ObservableObject
    {
        readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly IMessenger messenger;
        readonly IServiceProxy serviceProxy;

        ConnectionIndicatorViewModel _service;
        ConnectionIndicatorViewModel _connectionHES;
        ConnectionIndicatorViewModel _connectionRFID;
        ConnectionIndicatorViewModel _connectionDongle;

        public ConnectionIndicatorViewModel Service
        {
            get { return _service; }
            set { Set(ref _service, value); }
        }

        public ObservableCollection<ConnectionIndicatorViewModel> Indicators { get; } = new ObservableCollection<ConnectionIndicatorViewModel>();

        public IndicatorsViewModel(IMessenger messenger, IServiceProxy serviceProxy)
        {
            this.messenger = messenger;
            this.serviceProxy = serviceProxy;

            Service = new ConnectionIndicatorViewModel
            {
                Name = "Status.Service",
                HasConnectionText = "Status.Tooltip.ConectedService",
                NoConnectionText = "Status.Tooltip.DisconectedService",
            };

            _connectionHES = new ConnectionIndicatorViewModel
            {
                Name = "Status.HES",
                HasConnectionText = "Status.Tooltip.ConectedHES",
                NoConnectionText = "Status.Tooltip.DisconectedHES",
            };
            Indicators.Add(_connectionHES);

            _connectionRFID = new ConnectionIndicatorViewModel
            {
                Name = "Status.RFID",
                HasConnectionText = "Status.Tooltip.ConectedRFID",
                NoConnectionText = "Status.Tooltip.DisconectedRFID",
            };
            Indicators.Add(_connectionRFID);

            _connectionDongle = new ConnectionIndicatorViewModel
            {
                Name = "Status.Dongle",
                HasConnectionText = "Status.Tooltip.ConectedDongle",
                NoConnectionText = "Status.Tooltip.DisconectedDongle",
            };
            Indicators.Add(_connectionDongle);

            messenger.Register<ConnectionServiceChangedMessage>(this, OnConnectionServiceChanged);
            messenger.Register<ServiceComponentsStateChangedMessage>(this, OnServiceComponentsStateChanged);
        }

        void OnConnectionServiceChanged(ConnectionServiceChangedMessage message)
        {
            ResetIndicators();
        }

        void OnServiceComponentsStateChanged(ServiceComponentsStateChangedMessage message)
        {
            _connectionHES.State = message.HesConnected;
            _connectionHES.Visible = message.ShowHesStatus;

            _connectionRFID.State = message.RfidConnected;
            _connectionRFID.Visible = message.ShowRfidStatus;

            _connectionDongle.State = message.BleConnected;
        }

        void ResetIndicators()
        {
            try
            {
                Service.State = serviceProxy.IsConnected;
                _connectionDongle.State = false;
                _connectionRFID.State = false;
                _connectionRFID.Visible = false;
                _connectionHES.State = false;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
