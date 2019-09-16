using HideezClient.Models;
using HideezClient.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.PageViewModels
{
    class PasswordManagerViewModel : LocalizedObject
    {
        private Device device;
        public Device Device
        {
            get { return device; }
            set { Set(ref device, value); }
        }
    }
}
