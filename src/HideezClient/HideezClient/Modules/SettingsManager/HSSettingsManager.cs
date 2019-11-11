using GalaSoft.MvvmLight.Messaging;
using HideezMiddleware.Settings;
using HideezClient.Messages;

namespace HideezClient.Modules
{
    class HSSettingsManager<T> : SettingsManager<T>, ISettingsManager<T> where T : BaseSettings, new()
    {
        private readonly IMessenger messenger;

        /// <summary>
        /// Initializes a new instance of <see cref="HSSettingsManager"/> class
        /// </summary>
        /// <param name="settingsFilePath">Path to the settings file</param>
        public HSSettingsManager(string settingsFilePath, IFileSerializer fileSerializer, IMessenger messenger)
            :base(settingsFilePath, fileSerializer)
        {
            this.messenger = messenger;

            this.SettingsChanged += HSSettingsManager_SettingsChanged;
        }

        private void HSSettingsManager_SettingsChanged(object sender, SettingsChangedEventArgs<T> e)
        {
            messenger.Send(new SettingsChangedMessage<T>());
        }
    }
}
