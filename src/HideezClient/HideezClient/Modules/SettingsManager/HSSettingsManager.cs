using GalaSoft.MvvmLight.Messaging;
using HideezMiddleware.Settings;
using HideezClient.Messages;
using Meta.Lib.Modules.PubSub;

namespace HideezClient.Modules
{
    class HSSettingsManager<T> : SettingsManager<T>, ISettingsManager<T> where T : BaseSettings, new()
    {
        readonly IMetaPubSub _metaMessenger;

        /// <summary>
        /// Initializes a new instance of <see cref="HSSettingsManager"/> class
        /// </summary>
        /// <param name="settingsFilePath">Path to the settings file</param>
        public HSSettingsManager(string settingsFilePath, IFileSerializer fileSerializer, IMetaPubSub metaMessenger)
            :base(settingsFilePath, fileSerializer)
        {
            _metaMessenger = metaMessenger;

            this.SettingsChanged += HSSettingsManager_SettingsChanged;
        }

        private void HSSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<T> e)
        {
            _metaMessenger.Publish(new SettingsChangedMessage<T>());
        }
    }
}
