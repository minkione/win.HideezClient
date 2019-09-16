using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HideezClient.Controls
{
    /// <summary>
    /// Interaction logic for StateControl.xaml
    /// </summary>
    public partial class StateControl : UserControl
    {
        public StateControl()
        {
            InitializeComponent();
        }

        private void UpdateState()
        {
            if (State)
            {
                elipse.Fill = HasConnectionBackground;
                elipse.Effect = EffectConnected;
                ToolTip = HasConnectionText;
            }
            else
            {
                elipse.Fill = NoConnectionBackground;
                elipse.Effect = EffectDisconnected;
                ToolTip = NoConnectionText;
            }
        }

        public string Header
        {
            get { return (string)GetValue(HederProperty); }
            set { SetValue(HederProperty, value); }
        }

        public static readonly DependencyProperty HederProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(StateControl)
                , new PropertyMetadata("", PropertyHeaderChangedCallback));

        private static void PropertyHeaderChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StateControl stateControl)
            {
                stateControl.header.Text = e.NewValue.ToString();
            }
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StateControl stateControl)
            {
                stateControl.UpdateState();
            }
        }

        public bool State
        {
            get { return (bool)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(nameof(State), typeof(bool), typeof(StateControl)
                , new PropertyMetadata(false, PropertyChangedCallback));
        
        public Brush HasConnectionBackground
        {
            get { return (Brush)GetValue(HasConnectionBackgroundProperty); }
            set { SetValue(HasConnectionBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HasConnectionBackgroundProperty =
            DependencyProperty.Register(nameof(HasConnectionBackground), typeof(Brush), typeof(StateControl)
                , new PropertyMetadata(Brushes.Green, PropertyChangedCallback));

        public Brush NoConnectionBackground
        {
            get { return (Brush)GetValue(NoConnectionBackgroundProperty); }
            set { SetValue(NoConnectionBackgroundProperty, value); }
        }

        public static readonly DependencyProperty NoConnectionBackgroundProperty =
            DependencyProperty.Register(nameof(NoConnectionBackground), typeof(Brush), typeof(StateControl)
                , new PropertyMetadata(Brushes.Red, PropertyChangedCallback));

        public string HasConnectionText
        {
            get { return (string)GetValue(HasConnectionTextProperty); }
            set { SetValue(HasConnectionTextProperty, value); }
        }

        public static readonly DependencyProperty HasConnectionTextProperty =
            DependencyProperty.Register(nameof(HasConnectionText), typeof(string), typeof(StateControl)
                , new PropertyMetadata("", PropertyChangedCallback));

        public string NoConnectionText
        {
            get { return (string)GetValue(NoConnectionTextProperty); }
            set { SetValue(NoConnectionTextProperty, value); }
        }

        public static readonly DependencyProperty NoConnectionTextProperty =
            DependencyProperty.Register(nameof(NoConnectionText), typeof(string), typeof(StateControl)
                , new PropertyMetadata("", PropertyChangedCallback));

        public Effect EffectConnected
        {
            get { return (Effect)GetValue(EffectConnectedProperty); }
            set { SetValue(EffectConnectedProperty, value); }
        }

        public static readonly DependencyProperty EffectConnectedProperty =
            DependencyProperty.Register(nameof(EffectConnected), typeof(Effect), typeof(StateControl), new PropertyMetadata(null, PropertyChangedCallback));

        public Effect EffectDisconnected
        {
            get { return (Effect)GetValue(EffectDisconnectedProperty); }
            set { SetValue(EffectDisconnectedProperty, value); }
        }

        public static readonly DependencyProperty EffectDisconnectedProperty =
            DependencyProperty.Register(nameof(EffectDisconnected), typeof(Effect), typeof(StateControl), new PropertyMetadata(null, PropertyChangedCallback));

    }
}
