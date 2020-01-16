using Hideez.ARM;
using System.Windows;
using System.Windows.Controls;

namespace TemplateFactory
{
    /// <summary>
    /// Interaction logic for CopyableAppInfo.xaml
    /// </summary>
    public partial class CopyableAppInfo : UserControl
    {
        public AppInfo AppInfo
        {
            get { return (AppInfo)GetValue(AppInfoProperty); }
            set { SetValue(AppInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppInfoProperty =
            DependencyProperty.Register(
                "AppInfo", 
                typeof(AppInfo), 
                typeof(CopyableAppInfo), 
                new PropertyMetadata(new AppInfo())
                );

        public CopyableAppInfo()
        {
            InitializeComponent();
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard();
        }

        void CopyToClipboard()
        {
            string textToCopy = AppInfo.ProcessName.Trim();

            if (!string.IsNullOrWhiteSpace(AppInfo.Description))
                textToCopy = AppInfo.Description.Trim();

            Clipboard.SetText(textToCopy);
        }
    }
}
