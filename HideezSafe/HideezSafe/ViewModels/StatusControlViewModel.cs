using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Mvvm;

namespace HideezSafe.ViewModels
{
    class StatusControlViewModel : LocalizedObject
    {
        private string headerKey;
        private string falseToolTipKey;
        private string trueToolTipKey;
        private bool state;

        [Localization]
        public string Header
        {
            get { return L(headerKey); }
            set { Set(ref headerKey, value); }
        }

        [Localization]
        public string FalseToolTip
        {
            get { return L(falseToolTipKey); }
            set { Set(ref falseToolTipKey, value); }
        }

        [Localization]
        public string TrueToolTip
        {
            get { return L(trueToolTipKey); }
            set { trueToolTipKey = value; }
        }

        public bool Status
        {
            get { return state; }
            set { Set(ref state, value); }
        }
    }
}
