using Hideez.ARM;
using MvvmExtentions;
using MvvmExtentions.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TemplateFactory
{
    class MainWindowViewModel : PropertyChangedImplementation
    {
        bool refreshing = false;

        public MainWindowViewModel() : base()
        {
            RefreshAppList();
        }

        public ObservableCollection<ExpandedAppInfo> Apps { get; } = new ObservableCollection<ExpandedAppInfo>();

        public bool Refreshing
        {
            get
            {
                return refreshing;
            }
            set
            {
                if (refreshing != value)
                {
                    refreshing = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand RefreshAppListCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        RefreshAppList();
                    }
                };
            }
        }

        async void RefreshAppList()
        {
            try
            {
                Refreshing = true;

                await Task.Run(() =>
                {
                    var apps = AppInfoFactory.GetVisibleAppsInfo();
                    var uniqueApps = apps.GroupBy(x => x.Description).Select(a => a.First());

                    var expandedApps = uniqueApps.Select(a => new ExpandedAppInfo(a)).ToList();

                    var urls = new List<ExpandedAppInfo>();
                    foreach (var app in expandedApps)
                    {
                        if (!string.IsNullOrWhiteSpace(app.AppInfo.Domain))
                        {
                            urls.Add(new ExpandedAppInfo(app.AppInfo.Copy()));
                            app.AppInfo.Domain = string.Empty;
                        }
                    }

                    expandedApps.AddRange(urls);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Apps.Clear();
                        foreach (var app in expandedApps)
                            Apps.Add(app);
                    });
                });
            }
            finally
            {
                Refreshing = false;
            }
        }
    }
}
