using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Mvvm;
using MvvmExtentions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.ViewModels
{
    public class DeviceViewModel : LocalizedObject
    {
        public DeviceViewModel(string typeName, string icoKey, string name)
        {
            this.typeNameKey = typeName;
            this.IcoKey = icoKey;
            this.name = name;
        }

        #region Property

        public string IcoKey { get; }

        #region Text

        private string name;
        private string typeNameKey;

        [Localization]
        [DependsOn(nameof(TypeName))]
        public string Name
        {
            get { return $"{L(typeNameKey)} - {name}"; }
            set { Set(ref name, value); }
        }

        [Localization]
        public string TypeName
        {
            get { return L(typeNameKey); }
            set { Set(ref typeNameKey, value); }
        }

        #endregion Text

        #endregion Property
    }
}
