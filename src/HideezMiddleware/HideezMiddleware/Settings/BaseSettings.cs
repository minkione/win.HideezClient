using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HideezMiddleware.Settings
{
    [Serializable]
    public abstract class BaseSettings : ICloneable
    {
        /// <summary>
        /// Collection of properties that are marked as Settings
        /// </summary>
        [NonSerialized]
        private List<PropertyInfo> settingsProperties;

        /// <summary>
        /// Collection of properties that are marked as Settings
        /// </summary>
        protected List<PropertyInfo> SettingsProperties
        {
            get
            {
                if (settingsProperties == null)
                    settingsProperties = GetSettingsProperties();

                return settingsProperties;
            }
        }

        // All classes that inherit BaseSettings must override Clone for deep copies
        public abstract object Clone();

        public override bool Equals(object obj)
        {
            if (!(obj is BaseSettings other))
                return false;

            // Runtime types must be equal
            if (GetType() != obj.GetType())
                return false;

            var otherProperties = other.SettingsProperties;

            if (SettingsProperties.Count != otherProperties.Count)
                return false;

            var propertyDictionary = SettingsProperties.ToDictionary(p => p.Name, p => p.GetValue(this));
            var otherPropertyDictionary = SettingsProperties.ToDictionary(p => p.Name, p => p.GetValue(obj));

            return propertyDictionary.Except(otherPropertyDictionary).Count() == 0;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 43;
                int inc = 53;

                // Required as an additional variable for child classes with same properties
                hash = hash * inc + GetType().GetHashCode();

                foreach (var property in SettingsProperties)
                    hash = hash * inc + property.GetValue(this).GetHashCode();

                return hash;
            }
        }

        /// <summary>
        /// Get a collection of properties that are marked as Settings
        /// </summary>
        /// <returns>Returns a collection of properties that are marked as Settings</returns>
        private List<PropertyInfo> GetSettingsProperties()
        {
            var markedProperties = new List<PropertyInfo>(); // Properties marked with Setting attribute

            var properties = GetType().GetProperties();
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(typeof(SettingAttribute), false);

                if (attributes.Count() > 0)
                    markedProperties.Add(property);
            }

            return markedProperties;
        }
    }
}
