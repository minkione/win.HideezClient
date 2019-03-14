using GalaSoft.MvvmLight.Messaging;
using HideezSafe.Modules;
using HideezSafe.Mvvm.Messages;
using HideezSafe.Properties;
using System.Threading;

namespace HideezSafe.Mvvm
{
    class LanguageMessageHandler : ILanguageMessageHandler
    {
        public LanguageMessageHandler(IMessenger messenger)
        {
            messenger.Register<LanguageChangedMessage>(this, OnMessageReceived);
        }

        private void OnMessageReceived(LanguageChangedMessage languageChanged)
        {
            Settings.Default.Culture = languageChanged.NewCulture;
            Settings.Default.Save();

            TranslationSource.Instance.CurrentCulture = Settings.Default.Culture;

            Thread.CurrentThread.CurrentCulture = languageChanged.NewCulture;
            Thread.CurrentThread.CurrentUICulture = languageChanged.NewCulture;
        }
    }
}
