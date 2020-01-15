using Hideez.ARM;
using MvvmExtentions;
using MvvmExtentions.Commands;
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
                    // TODO: AppInfo is returned with empty ProcessId
                    var apps = AppInfoFactory.GetVisibleAppsInfo();
                    var uniqueApps = apps.GroupBy(x => x.Description).Select(a => a.First());

                    var expandedApps = uniqueApps.Select(a => new ExpandedAppInfo(a)).ToList();

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
