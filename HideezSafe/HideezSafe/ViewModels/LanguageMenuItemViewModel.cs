using HideezSafe.Modules;
using System;

namespace HideezSafe.ViewModels
{
    class LanguageMenuItemViewModel : MenuItemViewModel
    {
        public override bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            IsChecked = Header.Equals(TranslationSource.Instance.CurrentCulture.NativeName, StringComparison.OrdinalIgnoreCase);

            return base.ReceiveWeakEvent(managerType, sender, e);
        }
    }
}
