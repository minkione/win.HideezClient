using HideezClient.Models.Settings;
using HideezClient.Mvvm;
using HideezMiddleware.Settings;

namespace HideezClient.ViewModels
{
    class IndicatorsSettingViewModel : LocalizedObject
    {
        readonly ISettingsManager<ApplicationSettings> _appSettingsManager;

        public bool ShowDongleIndicator
        {
            get { return _appSettingsManager.Settings.ShowDongleIndicator; }
            set
            {
                var settings = _appSettingsManager.Settings;
                settings.ShowDongleIndicator = !settings.ShowDongleIndicator;
                _appSettingsManager.SaveSettings(settings);
            }
        }

        public bool ShowBluetoothIndicator
        {
            get { return _appSettingsManager.Settings.ShowBluetoothIndicator; }
            set
            {
                var settings = _appSettingsManager.Settings;
                settings.ShowBluetoothIndicator = !settings.ShowBluetoothIndicator;
                _appSettingsManager.SaveSettings(settings);
            }
        }

        public IndicatorsSettingViewModel(ISettingsManager<ApplicationSettings> appSettingsManager)
        {
            _appSettingsManager = appSettingsManager;
            _appSettingsManager.SettingsChanged += AppSettingsManager_SettingsChanged;
        }

        private void AppSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<ApplicationSettings> e)
        {
            NotifyPropertyChanged(nameof(ShowDongleIndicator));
            NotifyPropertyChanged(nameof(ShowBluetoothIndicator));
        }
    }
}
