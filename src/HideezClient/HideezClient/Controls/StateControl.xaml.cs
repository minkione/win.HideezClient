using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace HideezClient.Controls
{
    public enum StateControlState
    {
        Green,
        Orange,
        Red,
    }

    /// <summary>
    /// Interaction logic for StateControl.xaml
    /// </summary>
    public partial class StateControl : UserControl
    {
        public StateControl()
        {
            InitializeComponent();
        }

        static void PropertyHeaderChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StateControl stateControl)
            {
                stateControl.header.Text = e.NewValue.ToString();
            }
        }

        static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StateControl stateControl)
            {
                stateControl.UpdateState();
            }
        }

        void UpdateState()
        {
            switch (State)
            {
                case StateControlState.Green:
                    elipse.Fill = GreenBackground;
                    elipse.Effect = GreenEffect;
                    if (!string.IsNullOrWhiteSpace(GreenTooltip))
                        ToolTip = GreenTooltip;
                    break;
                case StateControlState.Orange:
                    elipse.Fill = OrangeBackground;
                    elipse.Effect = OrangeEffect;
                    if (!string.IsNullOrWhiteSpace(OrangeTooltip))
                        ToolTip = OrangeTooltip;
                    break;
                case StateControlState.Red:
                    elipse.Fill = RedBackground;
                    elipse.Effect = RedEffect;
                    if (!string.IsNullOrWhiteSpace(RedTooltip))
                        ToolTip = RedTooltip;
                    break;

            }
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public StateControlState State
        {
            get { return (StateControlState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public Brush GreenBackground
        {
            get { return (Brush)GetValue(GreenBackgroundProperty); }
            set { SetValue(GreenBackgroundProperty, value); }
        }
        public string GreenTooltip
        {
            get { return (string)GetValue(GreenTooltipProperty); }
            set { SetValue(GreenTooltipProperty, value); }
        }
        public Effect GreenEffect
        {
            get { return (Effect)GetValue(GreenEffectProperty); }
            set { SetValue(GreenEffectProperty, value); }
        }

        public Brush OrangeBackground
        {
            get { return (Brush)GetValue(OrangeBackgroundProperty); }
            set { SetValue(OrangeBackgroundProperty, value); }
        }
        public string OrangeTooltip
        {
            get { return (string)GetValue(OrangeTooltipProperty); }
            set { SetValue(OrangeTooltipProperty, value); }
        }
        public Effect OrangeEffect
        {
            get { return (Effect)GetValue(OrangeEffectProperty); }
            set { SetValue(OrangeEffectProperty, value); }
        }

        public Brush RedBackground
        {
            get { return (Brush)GetValue(RedBackgroundProperty); }
            set { SetValue(RedBackgroundProperty, value); }
        }
        public string RedTooltip
        {
            get { return (string)GetValue(RedTooltipProperty); }
            set { SetValue(RedTooltipProperty, value); }
        }
        public Effect RedEffect
        {
            get { return (Effect)GetValue(RedEffectProperty); }
            set { SetValue(RedEffectProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(StateControl), new PropertyMetadata("", PropertyHeaderChangedCallback));
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(StateControlState), typeof(StateControl), new PropertyMetadata(StateControlState.Red, PropertyChangedCallback));

        public static readonly DependencyProperty GreenBackgroundProperty = DependencyProperty.Register(nameof(GreenBackground), typeof(Brush), typeof(StateControl), new PropertyMetadata(Brushes.Green, PropertyChangedCallback));
        public static readonly DependencyProperty GreenTooltipProperty = DependencyProperty.Register(nameof(GreenTooltip), typeof(string), typeof(StateControl), new PropertyMetadata("", PropertyChangedCallback));
        public static readonly DependencyProperty GreenEffectProperty = DependencyProperty.Register(nameof(GreenEffect), typeof(Effect), typeof(StateControl), new PropertyMetadata(null, PropertyChangedCallback));

        public static readonly DependencyProperty OrangeBackgroundProperty = DependencyProperty.Register(nameof(OrangeBackground), typeof(Brush), typeof(StateControl), new PropertyMetadata(Brushes.Green, PropertyChangedCallback));
        public static readonly DependencyProperty OrangeTooltipProperty = DependencyProperty.Register(nameof(OrangeTooltip), typeof(string), typeof(StateControl), new PropertyMetadata("", PropertyChangedCallback));
        public static readonly DependencyProperty OrangeEffectProperty = DependencyProperty.Register(nameof(OrangeEffect), typeof(Effect), typeof(StateControl), new PropertyMetadata(null, PropertyChangedCallback));

        public static readonly DependencyProperty RedBackgroundProperty = DependencyProperty.Register(nameof(RedBackground), typeof(Brush), typeof(StateControl), new PropertyMetadata(Brushes.Green, PropertyChangedCallback));
        public static readonly DependencyProperty RedTooltipProperty = DependencyProperty.Register(nameof(RedTooltip), typeof(string), typeof(StateControl), new PropertyMetadata("", PropertyChangedCallback));
        public static readonly DependencyProperty RedEffectProperty = DependencyProperty.Register(nameof(RedEffect), typeof(Effect), typeof(StateControl), new PropertyMetadata(null, PropertyChangedCallback));
    }
}
