using Hideez.ARM;
using MvvmExtentions;
using System;
using System.Diagnostics;
using System.Drawing;

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
                    Icon = value;
                    NotifyPropertyChanged();
                }
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
