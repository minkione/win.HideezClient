using HideezSafe.Resources;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace HideezSafe.Modules
{
    /// <summary>
    /// A class that provide access to resource localized and manage one.
    /// </summary>
    public class TranslationSource : INotifyPropertyChanged
    {
        private IReadOnlyList<CultureInfo> supportedCultures;
        private readonly object supportedCulturesLockObj = new object();
        private readonly ResourceManager resManager = new ResourceManager(typeof(Strings));
        private CultureInfo currentCulture;

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
            get { return this.resManager.GetString(key, this.CurrentCulture); }
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

        /// <summary>
        /// Occurs when a property CurrentCulture value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
