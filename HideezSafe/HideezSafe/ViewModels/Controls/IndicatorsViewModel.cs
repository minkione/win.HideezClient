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
        private ConnectionIndicatorViewModel service;

        public ConnectionIndicatorViewModel Service
        {
            get { return service; }
            set { Set(ref service, value); }
        }

        public ObservableCollection<ConnectionIndicatorViewModel> Indicators { get; } = new ObservableCollection<ConnectionIndicatorViewModel>();

        public IndicatorsViewModel()
        {

            Service = new ConnectionIndicatorViewModel
            {
                Name = "Status.Service",
                HasConnectionText = "Status.Tooltip.ConectedService",
                NoConnectionText = "Status.Tooltip.DisconectedService",
            };

            Indicators.Add(new ConnectionIndicatorViewModel
            {
                Name = "Status.HES",
                HasConnectionText = "Status.Tooltip.ConectedHES",
                NoConnectionText = "Status.Tooltip.DisconectedHES",
            });
            Indicators.Add(new ConnectionIndicatorViewModel
            {
                Name = "Status.RFID",
                HasConnectionText = "Status.Tooltip.ConectedRFID",
                NoConnectionText = "Status.Tooltip.DisconectedRFID",
            });
            Indicators.Add(new ConnectionIndicatorViewModel
            {
                Name = "Status.Dongle",
                HasConnectionText = "Status.Tooltip.ConectedDongle",
                NoConnectionText = "Status.Tooltip.DisconectedDongle",
            });

#if DEBUG

            Service.State = true;

            Indicators[0].State = true;
            Indicators[1].State = false;
            Indicators[2].State = false;

#endif
        }
    }
}
