using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Mvvm;

namespace HideezSafe.ViewModels
{
    class ConnectionIndicatorViewModel : LocalizedObject
    {
        private string nameKey;
        private string noConnectionTextKey;
        private string hasConnectionTextKey;
        private bool state = false;
        private bool visible = true;

        [Localization]
        public string Name
        {
            get { return L(nameKey); }
            set { Set(ref nameKey, value); }
        }

        [Localization]
        public string NoConnectionText
        {
            get { return L(noConnectionTextKey); }
            set { Set(ref noConnectionTextKey, value); }
        }

        [Localization]
        public string HasConnectionText
        {
            get { return L(hasConnectionTextKey); }
            set { hasConnectionTextKey = value; }
        }

        public bool State
        {
            get { return state; }
            set { Set(ref state, value); }
        }

        public bool Visible
        {
            get { return visible; }
            set { Set(ref visible, value); }
        }
    }
}
