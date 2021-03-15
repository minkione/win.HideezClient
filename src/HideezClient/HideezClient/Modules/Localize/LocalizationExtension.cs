using HideezMiddleware.Localize;
using System.Windows.Data;

namespace HideezClient.Modules.Localize
{
    /// <summary>
    /// Binding localization key to resource localized.
    /// </summary>
    public class LocalizationExtension : Binding
    {
        public LocalizationExtension(string key) : base("[" + key + "]")
        {
            this.Mode = BindingMode.OneWay;
            this.Source = TranslationSource.Instance;
        }
    }
}
