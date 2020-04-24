using HideezClient.Utilities;
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
using Unity;

namespace HideezClient.Controls
{
    /// <summary>
    /// Interaction logic for InfoButton.xaml
    /// </summary>
    public partial class InfoButton : UserControl
    {
        IAppHelper _appHelper;

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public static readonly DependencyProperty UrlProperty = DependencyProperty.Register("Url", typeof(string), typeof(InfoButton), new PropertyMetadata(string.Empty));

        public InfoButton()
        {
            InitializeComponent();

            _appHelper = App.Container.Resolve<IAppHelper>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Url))
                _appHelper.OpenUrl(Url);
        }
    }
}
