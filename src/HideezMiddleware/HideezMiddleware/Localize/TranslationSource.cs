using HideezMiddleware.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace HideezMiddleware.Localize
{
    /// <summary>
    /// A class that provide access to resource localized and manage one.
    /// </summary>
    public class TranslationSource : INotifyPropertyChanged
    {
        private IReadOnlyList<CultureInfo> supportedCultures;
        private readonly object supportedCulturesLockObj = new object();
        private CultureInfo currentCulture;

        static List<ResourceManager> _resourceManagers = new List<ResourceManager>();

        protected TranslationSource()
        {
        }

        /// <summary>
        /// Provide single access to this source
        /// </summary>
        public static TranslationSource Instance { get; } = new TranslationSource();

        /// <summary>
        /// Returns the value of the string resource localized.
        /// </summary>
        /// <param name="key">Key to get string from resource localized.</param>
        /// <returns>Localized string.</returns>
        public string this[string key]
        {
            get
            {
                for(int i = _resourceManagers.Count-1; i>=0; i--)
                {
                    string result = _resourceManagers[i].GetString(key, CurrentCulture);

                    if (result != null)
                        return result;
                }
                
                return null;
            }
        }

        public string Format(string key, params object[] args)
        {
            return string.Format(this[key], args);
        }

        /// <summary>
        /// An object that represents the culture for which the resource is localized.
        /// </summary>
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

        /// <summary>
        /// Return supported cultures for resource localized.
        /// </summary>
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
                                foreach (var resManager in _resourceManagers)
                                {
                                    ResourceSet rs = resManager.GetResourceSet(culture, true, false);
                                    if (rs != null && !string.IsNullOrEmpty(culture.Name) && !listSupportedCultures.Contains(culture))
                                    {
                                        listSupportedCultures.Add(culture);
                                    }
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

        public static void SetResourceManagers(List<ResourceManager> resourceManagers)
        {
            _resourceManagers = resourceManagers;
        }

        /// <summary>
        /// Occurs when a property CurrentCulture value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
