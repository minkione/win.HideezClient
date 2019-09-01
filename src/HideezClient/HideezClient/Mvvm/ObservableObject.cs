using MvvmExtensions.PropertyChangedMonitoring;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HideezClient.Mvvm
{
    /// <summary>
    /// A base class for objects that notify clients that a property value has changed.
    /// </summary>
    public class ObservableObject : PropertyChangedImplementation
    {
        /// <summary>
        /// Assigns a new value to the property. Then, raises the PropertyChanged event if needed. 
        /// </summary>
        /// <typeparam name="T">The type of the property that changed.</typeparam>
        /// <param name="field">The field storing the property's value.</param>
        /// <param name="newValue">The property's value after the change occurred.</param>
        /// <param name="propertyName">(optional) The name of the property that changed.</param>
        /// <returns>True if the PropertyChanged event has been raised, false otherwise. 
        /// The event is not raised if the old value is equal to the new value.</returns>
        protected bool Set<T>(ref T field, T newValue = default(T), [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }
            T oldValue = field;
            field = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">(optional) The name of the property that changed.</param>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("This method cannot be called with an empty string", propertyName);
            }
            NotifyPropertyChanged(propertyName);
        }
    }
}
