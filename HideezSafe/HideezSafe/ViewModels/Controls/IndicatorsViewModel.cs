using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Messages;
using HideezSafe.Modules.ServiceCallbackMessanger;
using HideezSafe.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.ViewModels
{
    class IndicatorsViewModel : ObservableObject
    {
        private readonly IMessenger messenger;
        private ConnectionIndicatorViewModel service;

        public ConnectionIndicatorViewModel Service
        {
            get { return service; }
            set { Set(ref service, value); }
        }

        public ObservableCollection<ConnectionIndicatorViewModel> Indicators { get; } = new ObservableCollection<ConnectionIndicatorViewModel>();

        public IndicatorsViewModel(IMessenger messenger)
        {
            this.messenger = messenger;

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

            messenger.Register<ConnectionServiceChangedMessage>(Service, c => Service.State = c.IsConnected, true);
            messenger.Register<ConnectionDongleChangedMessage>(connectionDongle, c => connectionDongle.State = c.IsConnected, true);
            messenger.Register<ConnectionRFIDChangedMessage>(connectionRFID, c => connectionRFID.State = c.IsConnected, true);
            messenger.Register<ConnectionHESChangedMessage>(connectionHES, c => connectionHES.State = c.IsConnected, true);

//#if DEBUG
            //Service.State = true;
            //connectionDongle.State = true;
            //connectionRFID.State = false;
            //connectionHES.State = false;
//#endif
        }
    }
}
