using HideezClient.Controls;
using HideezMiddleware.Localize;
using HideezClient.Mvvm;
using HideezClient.Modules.Localize;

namespace HideezClient.ViewModels.Controls
{
    class StateControlViewModel : LocalizedObject
    {
        string nameKey = string.Empty;
        string redTooltipKey = string.Empty;
        string orangleTooltipKey = string.Empty;
        string greenTooltipKey = string.Empty;
        StateControlState state = StateControlState.Red;
        bool visible = false;

        [Localization]
        public string Name
        {
            get { return L(nameKey); }
            set { Set(ref nameKey, value); }
        }

        [Localization]
        public string RedTooltip
        {
            get { return L(redTooltipKey); }
            set { Set(ref redTooltipKey, value); }
        }

        [Localization]
        public string OrangeTooltip
        {
            get { return L(orangleTooltipKey); }
            set { Set(ref orangleTooltipKey, value); }
        }

        [Localization]
        public string GreenTooltip
        {
            get { return L(greenTooltipKey); }
            set { greenTooltipKey = value; }
        }

        public StateControlState State
        {
            get { return state; }
            set { Set(ref state, value); }
        }

        public bool Visible
        {
            get { return visible; }
            set { Set(ref visible, value); }
        }

        public static StateControlState BoolToState(bool value)
        {
            return value ? StateControlState.Green : StateControlState.Red;
        }
    }
}
