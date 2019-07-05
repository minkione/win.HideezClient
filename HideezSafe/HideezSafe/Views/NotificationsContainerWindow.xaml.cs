using HideezSafe.Controls;
using HideezSafe.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HideezSafe.Views
{
    /// <summary>
    /// Interaction logic for NotificationsWindow.xaml
    /// </summary>
    public partial class NotificationsContainerWindow : Window
    {
        private readonly string screenName;

        public NotificationsContainerWindow(Screen screen)
        {
            InitializeComponent();

            this.screenName = screen.DeviceName;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            if (notifyItems.Items is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged += NotificationsContainerWindow_CollectionChanged;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            UpdateWindowContainer();
        }

        private void NotificationsContainerWindow_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateWindowContainer();
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (NotificationBase item in e?.NewItems)
                {
                    if (item.Options.SetFocus)
                    {
                        this.Activate();
                        break;
                    }
                }
            }
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);

            foreach (var item in notifyItems.Items.OfType<NotificationBase>().Where(n => n.Options.CloseWhenDeactivate))
            {
                item.Close();
            }
        }

        private void UpdateWindowContainer()
        {
            Screen screen = Screen.AllScreens.FirstOrDefault(s => s.DeviceName == screenName);
            if (screen != null)
            {
                UpdateLayout();
                Height = screen.WorkingArea.Height;
                var point = GetPositionForBottomRightCorner(screen, ActualWidth, ActualHeight);
                Left = point.X;
                Top = point.Y;
            }
            //NotificationBase nb = notifyItems.Items.Cast<NotificationBase>().FirstOrDefault(n => n.Options.SetFocus);
            //nb?.Focus();
        }

        private (double X, double Y) GetPositionForBottomRightCorner(Screen screen, double actualWidth, double actualHeight)
        {
            var dpiTransform = GetDpiTransform();
            double width = actualWidth * dpiTransform.X;
            // double height = actualHeight * dpiTransform.Y;

            double pointX = screen.WorkingArea.Right - width;
            double pointY = screen.WorkingArea.Top;

            return (pointX / dpiTransform.X, pointY / dpiTransform.Y);
        }

        private (double X, double Y) GetDpiTransform()
        {
            PresentationSource source = PresentationSource.FromVisual(this);

            double dpiXFactor = 1;
            double dpiYFactor = 1;
            if (source != null)
            {
                dpiXFactor = source.CompositionTarget.TransformToDevice.M11;
                dpiYFactor = source.CompositionTarget.TransformToDevice.M22;
            }

            return (dpiXFactor, dpiYFactor);
        }

        #region HideWindow
        // Hide a WPF form from Alt+Tab

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_EX_STYLE = -20;
        private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Variable to hold the handle for the form
            var helper = new WindowInteropHelper(this).Handle;
            //Performing some magic to hide the form from Alt+Tab
            SetWindowLong(helper, GWL_EX_STYLE, (GetWindowLong(helper, GWL_EX_STYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
        }

        #endregion
    }
}
