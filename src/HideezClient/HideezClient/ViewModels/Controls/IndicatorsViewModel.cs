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
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IMessenger messenger;
        private readonly IServiceProxy serviceProxy;

        private ConnectionIndicatorViewModel service;
        private ConnectionIndicatorViewModel server;
        private ConnectionIndicatorViewModel rfid;
        private ConnectionIndicatorViewModel dongle;

        public IndicatorsViewModel(IMessenger messenger, IServiceProxy serviceProxy)
        {
            this.messenger = messenger;
            this.serviceProxy = serviceProxy;

            InitIndicators();

            messenger.Register<ConnectionServiceChangedMessage>(this, c => ResetIndicators(c.IsConnected), true);
            messenger.Register<ServiceComponentsStateChangedMessage>(this, message =>
            {
                Server.State = message.HesConnected;
                Server.Visible = message.ShowHesStatus;

                RFID.State = message.RfidConnected;
                RFID.Visible = message.ShowRfidStatus;

                Dongle.State = message.BleConnected;
            }
            , true);
        }

        #region Properties

        public ObservableCollection<ConnectionIndicatorViewModel> Indicators { get; } = new ObservableCollection<ConnectionIndicatorViewModel>();

        public ConnectionIndicatorViewModel Service
        {
            get { return _service; }
            set { Set(ref _service, value); }
        }

        public ConnectionIndicatorViewModel Server
        {
            get { return server; }
            set { Set(ref server, value); }
        }

        public ConnectionIndicatorViewModel RFID
        {
            get { return rfid; }
            set { Set(ref rfid, value); }
        }

        public ConnectionIndicatorViewModel Dongle
        {
            get { return dongle; }
            set { Set(ref dongle, value); }
        }

        #endregion
        
        private void InitIndicators()
        {
            Service = new ConnectionIndicatorViewModel
            {
                Name = "Status.Service",
                HasConnectionText = "Status.Tooltip.ConectedService",
                NoConnectionText = "Status.Tooltip.DisconectedService",
            };

            var connectionHES = new ConnectionIndicatorViewModel
            {
                Name = "Status.Server",
                HasConnectionText = "Status.Tooltip.ConectedServer",
                NoConnectionText = "Status.Tooltip.DisconectedServer",
            };

            var connectionRFID = new ConnectionIndicatorViewModel
            {
                Name = "Status.RFID",
                HasConnectionText = "Status.Tooltip.ConectedRFID",
                NoConnectionText = "Status.Tooltip.DisconectedRFID",
            };

            var connectionDongle = new ConnectionIndicatorViewModel
            {
                Name = "Status.Dongle",
                HasConnectionText = "Status.Tooltip.ConectedDongle",
                NoConnectionText = "Status.Tooltip.DisconectedDongle",
            };
            Indicators.Add(Server);
            Indicators.Add(RFID);
            Indicators.Add(Dongle);
        }

        private void ResetIndicators(bool isServiceisConnected)
        {
            try
            {
                Service.State = serviceProxy.IsConnected;
                Dongle.State = false;
                RFID.State = false;
                RFID.Visible = false;
                Server.State = false;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }
    }
}
