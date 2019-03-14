using System.Globalization;

namespace HideezSafe.Mvvm.Messages
{
    class LanguageChangedMessage
    {
        public LanguageChangedMessage(CultureInfo newCulture)
        {
           this.NewCulture = newCulture;
        }

        public CultureInfo NewCulture { get; }
    }
}
