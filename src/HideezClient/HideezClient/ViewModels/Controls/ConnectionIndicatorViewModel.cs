using HideezClient.Modules;
using HideezClient.Modules.Localize;
using HideezClient.Mvvm;

namespace HideezClient.ViewModels
{
    class ConnectionIndicatorViewModel : LocalizedObject
    {
        private bool state = false;
        private bool visible = true;
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
