using GalaSoft.MvvmLight.Messaging;
using Hideez.SDK.Communication.Log;
using HideezClient.Messages;
using HideezClient.Modules.Log;
using HideezClient.Modules.ServiceProxy;
using HideezClient.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace HideezClient.ViewModels
{
    class IndicatorsViewModel : ObservableObject
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger(nameof(IndicatorsViewModel));
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
            get { return service; }
            set { Set(ref service, value); }
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

            Server = new ConnectionIndicatorViewModel
            {
                Name = "Status.Server",
                HasConnectionText = "Status.Tooltip.ConectedServer",
                NoConnectionText = "Status.Tooltip.DisconectedServer",
            };

            RFID = new ConnectionIndicatorViewModel
            {
                Name = "Status.RFID",
                HasConnectionText = "Status.Tooltip.ConectedRFID",
                NoConnectionText = "Status.Tooltip.DisconectedRFID",
            };

            Dongle = new ConnectionIndicatorViewModel
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
                log.WriteLine(ex.Message);
            }
        }
    }
}
