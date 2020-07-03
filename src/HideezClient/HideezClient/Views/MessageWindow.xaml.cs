using Hideez.ARM;
using HideezClient.Utilities;
using System;
using System.Windows;
using System.Windows.Interop;

namespace HideezClient.Views
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        const int WM_CLOSE = 0x0010;

        IAppHelper _appHelper;

        public MessageWindow(IAppHelper appHelper)
        {
            _appHelper = appHelper;

            InitializeComponent();
            Top = SystemParameters.VirtualScreenTop + 1000;
            ShowInTaskbar = false;
            ShowActivated = false;
            Show();
            Hide();

            var handler = new WindowInteropHelper(this).EnsureHandle();
            HwndSource source = HwndSource.FromHwnd(handler);
            source.AddHook(WndProc);

            // Required for proper interaction with chromium-based browsers
            AutomationRegistrator.Instance.RegisterHook();
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle messages...
            if (msg == WM_CLOSE)
            {
                // Shutdown application
                _appHelper.Shutdown();
            }

            return IntPtr.Zero;
        }
    }
}
