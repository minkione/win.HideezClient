﻿using GalaSoft.MvvmLight.Messaging;
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
        private ConnectionIndicatorViewModel tbServer;

        public IndicatorsViewModel(IMessenger messenger, IServiceProxy serviceProxy)
        {
            this.messenger = messenger;
            this.serviceProxy = serviceProxy;

            InitIndicators();

            messenger.Register<ConnectionServiceChangedMessage>(this, c => ResetIndicators(c.IsConnected), true);
            messenger.Register<ServiceComponentsStateChangedMessage>(this, message =>
            {
                log.WriteLine("Updating components state indicators");
                Server.State = message.HesConnected;
                Server.Visible = message.ShowHesStatus;

                RFID.State = message.RfidConnected;
                RFID.Visible = message.ShowRfidStatus;

                Dongle.State = message.BleConnected;

                TBServer.State = message.TBHesConnected;
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

        public ConnectionIndicatorViewModel TBServer
        {
            get { return tbServer; }
            set { Set(ref tbServer, value); }
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

            TBServer = new ConnectionIndicatorViewModel
            {
                Name = "Status.Network",
                HasConnectionText = "Status.Tooltip.NetworkAvailable",
                NoConnectionText = "Status.Tooltip.NetworkUnavailable",
            };

            Indicators.Add(Server);
            Indicators.Add(RFID);
            Indicators.Add(Dongle);
        }

        private void ResetIndicators(bool isServiceConnected)
        {
            log.WriteLine("Resetting components state indicators");
            try
            {
                Service.State = isServiceConnected;
                Dongle.State = false;
                RFID.State = false;
                RFID.Visible = false;
                Server.State = false;
                TBServer.State = false;
            }
            catch (Exception ex)
            {
                log.WriteLine(ex.Message);
            }
        }
    }
}
