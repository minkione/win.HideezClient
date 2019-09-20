using HideezClient.Models;
using HideezClient.Mvvm;
using HideezClient.ViewModels;
using MvvmExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.PageViewModels
{
    class PasswordManagerViewModel : LocalizedObject
    {
        private DeviceViewModel device;
        public DeviceViewModel Device
        {
            get { return device; }
            set { Set(ref device, value); }
        }

        public ObservableCollection<AccountViewModel> Accounts { get; } = new ObservableCollection<AccountViewModel>
        {
            new AccountViewModel { Name = "Pizza Hut", Login = "john.gardner@example.com", HasOpt = true, },
            new AccountViewModel { Name = "The Walt Disney Company", Login = "seth.olson@example.com", },
            new AccountViewModel { Name = "Bank of America", Login = "penny.nichols@example.com", },
            new AccountViewModel { Name = "eBay", Login = "alice.bryant@example.com", },
            new AccountViewModel { Name = "MasterCard", Login = "tamara.kuhn@example.com", },
            new AccountViewModel { Name = "Johnson & Johnson", Login = "keith.richards@example.com", HasOpt = true, },
            new AccountViewModel { Name = "Starbucks", Login = "logan.hopkins@example.com", },
            new AccountViewModel { Name = "Facebook", Login = "kelly.howard@example.com", },
            new AccountViewModel { Name = "L'Oréal", Login = "jeff.anderson@example.com", },
            new AccountViewModel { Name = "Mitsubishi", Login = "dan.romero@example.com", HasOpt = true, },
            new AccountViewModel { Name = "Apple", Login = "gary.herrera@example.com", },
            //new AccountViewModel { Name = "Louis Vuitton", Login = "jessica.hanson@example.com", },
        };
    }

    class AccountViewModel : LocalizedObject
    {
        private string name;
        private string login;
        private bool hasOpt;
        private string searchQuery;

        private ObservableCollection<string> webSiteApp = new ObservableCollection<string>()
        {
            "facebook.com",
            "facebook.com",
            "facebook.com",
            "facebook.com",
        };

        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        public string Login
        {
            get { return login; }
            set { Set(ref login, value); }
        }

        [DependsOn("HasOpt")]
        public string Otp
        {
            get { return HasOpt ? "enabled" : "disabled"; }
        }

        public bool HasOpt
        {
            get { return hasOpt; }
            set { hasOpt = value; }
        }

        public ObservableCollection<string> WebSiteApp
        {
            get { return webSiteApp; }
            set { Set(ref webSiteApp, value); }
        }

        public string SearchQuery
        {
            get { return searchQuery; }
            set { Set(ref searchQuery, value); }
        }
    }
}
