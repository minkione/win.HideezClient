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
        private ConnectionIndicatorViewModel service = new ConnectionIndicatorViewModel();
        private ConnectionIndicatorViewModel server = new ConnectionIndicatorViewModel();
        private ConnectionIndicatorViewModel rfid = new ConnectionIndicatorViewModel();
        private ConnectionIndicatorViewModel dongle = new ConnectionIndicatorViewModel();

        public IndicatorsViewModel(IMessenger messenger, IServiceProxy serviceProxy)
        {
            this.messenger = messenger;
            this.serviceProxy = serviceProxy;

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
