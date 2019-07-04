using GalaSoft.MvvmLight.Messaging;
using HideezSafe.HideezServiceReference;
using HideezSafe.Messages;
using HideezSafe.Modules.ServiceCallbackMessanger;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Mvvm;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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

            async void InitIndicators()
            {
                try
                {
                    if (serviceProxy.IsConnected)
                    {
                        IHideezService hideezService = serviceProxy.GetService();
                        Service.State = serviceProxy.IsConnected;
                        connectionDongle.State = await hideezService.GetAdapterStateAsync(Adapter.Dongle);
                        connectionRFID.State = await hideezService.GetAdapterStateAsync(Adapter.RFID);
                        connectionHES.State = await hideezService.GetAdapterStateAsync(Adapter.HES);
                    }
                    else
                    {
                        Service.State = false;
                        connectionDongle.State = false;
                        connectionRFID.State = false;
                        connectionHES.State = false;
                    }
                }
                catch (FaultException<HideezServiceFault> ex)
                {
                    log.Error(ex.FormattedMessage());
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                }
            }

            messenger.Register<ConnectionServiceChangedMessage>(Service, c => InitIndicators(), true);
            messenger.Register<ConnectionDongleChangedMessage>(connectionDongle, c => connectionDongle.State = c.IsConnected, true);
            messenger.Register<ConnectionRFIDChangedMessage>(connectionRFID, c => connectionRFID.State = c.IsConnected, true);
            messenger.Register<ConnectionHESChangedMessage>(connectionHES, c => connectionHES.State = c.IsConnected, true);
        }
    }
}
