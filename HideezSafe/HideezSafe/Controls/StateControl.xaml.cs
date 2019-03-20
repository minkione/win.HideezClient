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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HideezSafe.Controls
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
                elipse.Fill = TrueBackground;
                icon.Kind = MahApps.Metro.IconPacks.PackIconOcticonsKind.Check;
                ToolTip = TrueToolTip;
            }
            else
            {
                elipse.Fill = FalseBackground;
                icon.Kind = MahApps.Metro.IconPacks.PackIconOcticonsKind.X;
                ToolTip = FalseToolTip;
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
        
        public Brush TrueBackground
        {
            get { return (Brush)GetValue(TrueBackgroundProperty); }
            set { SetValue(TrueBackgroundProperty, value); }
        }

        public static readonly DependencyProperty TrueBackgroundProperty =
            DependencyProperty.Register(nameof(TrueBackground), typeof(Brush), typeof(StateControl)
                , new PropertyMetadata(Brushes.Green, PropertyChangedCallback));

        public Brush FalseBackground
        {
            get { return (Brush)GetValue(FalseBackgroundProperty); }
            set { SetValue(FalseBackgroundProperty, value); }
        }

        public static readonly DependencyProperty FalseBackgroundProperty =
            DependencyProperty.Register(nameof(FalseBackground), typeof(Brush), typeof(StateControl)
                , new PropertyMetadata(Brushes.Red, PropertyChangedCallback));

        public string TrueToolTip
        {
            get { return (string)GetValue(TrueToolTipProperty); }
            set { SetValue(TrueToolTipProperty, value); }
        }

        public static readonly DependencyProperty TrueToolTipProperty =
            DependencyProperty.Register(nameof(TrueToolTip), typeof(string), typeof(StateControl)
                , new PropertyMetadata("", PropertyChangedCallback));

        public string FalseToolTip
        {
            get { return (string)GetValue(FalsePopupProperty); }
            set { SetValue(FalsePopupProperty, value); }
        }

        public static readonly DependencyProperty FalsePopupProperty =
            DependencyProperty.Register(nameof(FalseToolTip), typeof(string), typeof(StateControl)
                , new PropertyMetadata("", PropertyChangedCallback));
    }
}
