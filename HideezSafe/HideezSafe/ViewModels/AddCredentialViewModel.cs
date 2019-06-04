using HideezSafe.Modules;
using HideezSafe.Modules.ServiceProxy;
using HideezSafe.Mvvm;
using HideezSafe.Utilities;
using HideezSafe.Views;
using MvvmExtensions.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HideezSafe.ViewModels
{
    class AddCredentialViewModel : ObservableObject
    {
        private readonly IServiceProxy serviceProxy;
        private readonly IWindowsManager windowsManager;

        public AddCredentialViewModel(IServiceProxy serviceProxy, IWindowsManager windowsManager)
        {
            this.serviceProxy = serviceProxy;
            this.windowsManager = windowsManager;
            logins = new ObservableCollection<string>(GetAllUserNames());
        }

        private List<string> GetAllUserNames()
        {
            List<string> result = new List<string>();

            // Get all "real" local usernames
            SelectQuery query = new SelectQuery("Select * from Win32_UserAccount Where LocalAccount = True");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            var localUsers = searcher.Get().Cast<ManagementObject>().Where(
                u => (bool)u["LocalAccount"] == true &&
                     (bool)u["Disabled"] == false &&
                     (bool)u["Lockout"] == false &&
                     int.Parse(u["SIDType"].ToString()) == 1 &&
                     u["Name"].ToString() != "HomeGroupUser$");

            // Try to get MS Account for each local username and if found use it instead of local username
            foreach (ManagementObject user in localUsers)
            {
                string msName = LocalToMSAccountConverter.TryTransformToMS(user["Name"] as string);

                if (!String.IsNullOrWhiteSpace(msName))
                    result.Add(@"MicrosoftAccount\" + msName);
                else
                    result.Add(new SecurityIdentifier(user["SID"].ToString()).Translate(typeof(NTAccount)).ToString());
            }

            return result;
        }

        #region Command

        public ICommand SaveCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        if (x is AddCredentialView view)
                        {
                            IsInProgress = true;
                            var login = SelectedLogin;
                            var pass = view.passwordBox.Password;


                            Task.Run(async () =>
                            {
                                try
                                {
                                    await serviceProxy.GetService().SaveCredentialAsync(DeviceId, login, pass);

                                    IsInProgress = false;
                                }
                                catch(Exception ex)
                                {
                                    windowsManager.ShowError(LocalizedObject.L("Error.SaveCredential"));

                                }
                                finally
                                {
                                    Application.Current.Dispatcher.Invoke(view.Close);
                                }
                            });
                        }
                    },
                };
            }
        }

        #endregion Command

        #region Properties

        private readonly ObservableCollection<string> logins;


        public ObservableCollection<string> Logins
        {
            get { return logins; }
        }


        private string selectedLogin;

        public string SelectedLogin
        {
            get { return selectedLogin; }
            set
            {
                Set(ref selectedLogin, value);
            }
        }

        public string NewItem
        {
            set
            {
                if (SelectedLogin != null)
                {
                    return;
                }
                if (!string.IsNullOrEmpty(value))
                {
                    Logins.Add(value);
                    SelectedLogin = value;
                }
            }
        }


        private bool isInProgress;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set
            {
                Set(ref isInProgress, value);
            }
        }

        public string DeviceId { get; set; }

        #endregion Properties
    }
}
