using GalaSoft.MvvmLight.Helpers;
using HideezSafe.Resources;
using HideezSafe.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Linq;
using System.Windows;

namespace HideezSafe.Modules
{
    public class TranslationSource : INotifyPropertyChanged
    {
        private static readonly TranslationSource instance = new TranslationSource();
        private IReadOnlyList<CultureInfo> supportedCultures;
        private readonly object supportedCulturesLockObj = new object();

        protected TranslationSource()
        {
        }

        public static TranslationSource Instance
        {
            get { return instance; }
        }

        private readonly ResourceManager resManager = new ResourceManager(typeof(Strings));
        private CultureInfo currentCulture;

        public string this[string key]
        {
            get { return this.resManager.GetString(key, this.CurrentCulture); }
        }

        public CultureInfo CurrentCulture
        {
            get { return this.currentCulture; }
            set
            {
                if (this.currentCulture != value)
                {
                    this.currentCulture = value;
                    var @event = this.PropertyChanged;
                    if (@event != null)
                    {
                        @event.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                    }
                }
            }
        }

        public IReadOnlyList<CultureInfo> SupportedCultures
        {
            get
            {
                lock (supportedCulturesLockObj)
                {
                    if (supportedCultures == null)
                    {
                        List<CultureInfo> listSupportedCultures = new List<CultureInfo>();
                        CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                        foreach (CultureInfo culture in cultures)
                        {
                            try
                            {
                                ResourceSet rs = resManager.GetResourceSet(culture, true, false);
                                if (rs != null && !string.IsNullOrEmpty(culture.Name))
                                {
                                    listSupportedCultures.Add(culture);
                                }
                            }
                            catch (CultureNotFoundException)
                            {
                            }
                        }

                        supportedCultures = listSupportedCultures;
                    }

                    return supportedCultures;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
