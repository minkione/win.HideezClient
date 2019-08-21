using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Messages;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Mvvm;
using NLog;
using System;
using System.Collections.ObjectModel;

namespace HideezSafe.ViewModels
{
    class IndicatorsViewModel : ObservableObject
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IMessenger messenger;
        private readonly IServiceProxy serviceProxy;
        private ConnectionIndicatorViewModel service;

        public ConnectionIndicatorViewModel Service
        {
            get { return service; }
            set { Set(ref service, value); }
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

            var connectionHES = new ConnectionIndicatorViewModel
            {
                Name = "Status.HES",
                HasConnectionText = "Status.Tooltip.ConectedHES",
                NoConnectionText = "Status.Tooltip.DisconectedHES",
            };
            Indicators.Add(connectionHES);

            var connectionRFID = new ConnectionIndicatorViewModel
            {
                Name = "Status.RFID",
                HasConnectionText = "Status.Tooltip.ConectedRFID",
                NoConnectionText = "Status.Tooltip.DisconectedRFID",
            };
            Indicators.Add(connectionRFID);

            var connectionDongle = new ConnectionIndicatorViewModel
            {
                Name = "Status.Dongle",
                HasConnectionText = "Status.Tooltip.ConectedDongle",
                NoConnectionText = "Status.Tooltip.DisconectedDongle",
            };
            Indicators.Add(connectionDongle);

            void ResetIndicators()
            {
                try
                {
                    Service.State = serviceProxy.IsConnected;
                    connectionDongle.State = false;
                    connectionRFID.State = false;
                    connectionHES.State = false;
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                }
            }

            messenger.Register<ConnectionServiceChangedMessage>(Service, c => ResetIndicators(), true);
            messenger.Register<ServiceComponentsStateChangedMessage>(connectionDongle, message =>
            {
                connectionDongle.State = message.BleConnected;
                connectionRFID.State = message.RfidConnected;
                connectionHES.State = message.HesConnected;
            }
            , true);
        }
    }
}
