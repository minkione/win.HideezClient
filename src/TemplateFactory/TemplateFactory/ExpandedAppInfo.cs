using Hideez.ARM;
using MvvmExtensions.PropertyChangedMonitoring;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TemplateFactory
{
    public class ExpandedAppInfo : PropertyChangedImplementation
    {
        AppInfo appInfo;
        Icon icon;

        public AppInfo AppInfo
        {
            get
            {
                return appInfo;
            }
            set
            {
                if (appInfo != value)
                {
                    appInfo = value;
                    NotifyPropertyChanged();
                };
            }
        }

        public Icon Icon
        {
            get
            {
                return icon;
            }
            set
            {
                if (icon != value)
                {
                    icon = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ImageSource IconImage
        {
            get
            {
                return Icon.ToImageSource();
            }
        }

        public ExpandedAppInfo(AppInfo info)
        {
            AppInfo = info;
            try
            {
                var proc = Process.GetProcessById(AppInfo.ProcessId);

                if (proc != null)
                    Icon = ProcessExtensions.GetIcon(proc);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
