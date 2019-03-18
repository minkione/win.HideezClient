using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using HideezSafe.Modules;

namespace HideezSafe.Mvvm
{
    /// <summary>
    /// A class provides localization extension.
    /// </summary>
    class LocalizedObject : ObservableObject, IWeakEventListener
    {
        private List<string> localizeDependencies;

        public LocalizedObject()
        {
            RegisterLocalizeDependencies();

            // subscribe to localize changed event
            PropertyChangedEventManager.AddListener(TranslationSource.Instance, this, string.Empty);
        }

        /// <summary>
        /// Gets a localized string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The resolved localized string.</returns>
        public static string L(string key)
        {
            return TranslationSource.Instance[key] ?? key;
        }

        /// <summary>
        /// Event handler for localized culture changed.
        /// </summary>
        public virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            localizeDependencies.ForEach(RaisePropertyChanged);

            return true;
        }

        /// <summary>
        /// Cleans up the instance, remove locale change listener.
        /// </summary>
        public virtual void Cleanup()
        {
            PropertyChangedEventManager.RemoveListener(TranslationSource.Instance, this, string.Empty);
        }

        /// <summary>
        /// Find all properties with LocalizationAttribute.
        /// Create weak event and subscribe to localize changed event.
        /// </summary>
        private void RegisterLocalizeDependencies()
        {
            localizeDependencies = GetType().GetProperties()
                    .Where(pi => pi.GetCustomAttributes(typeof(LocalizationAttribute), false).Length != 0)
                    .Select(pi => pi.Name).ToList();
        }
    }
}
