using HideezClient.ViewModels.Controls;
using ReactiveUI.Fody.Helpers;
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

namespace HideezClient.Controls
{
   
    /// <summary>
    /// Interaction logic for ProgressIndicatorWithResult.xaml
    /// </summary>
    public partial class ProgressIndicatorWithResultControl : UserControl
    {
        public ProgressIndicatorWithResultViewModel ProcessResult
        {
            get { return (ProgressIndicatorWithResultViewModel)GetValue(ProcessResultProperty); }
            set { SetValue(ProcessResultProperty, value); }
        }

        public static readonly DependencyProperty ProcessResultProperty =
            DependencyProperty.Register(
                "ProcessResult",
                typeof(ProgressIndicatorWithResultViewModel),
                typeof(ProgressIndicatorWithResultControl),
                new PropertyMetadata(null)
                );

        public ProgressIndicatorWithResultControl()
        {
            InitializeComponent();
        }
    }
}
