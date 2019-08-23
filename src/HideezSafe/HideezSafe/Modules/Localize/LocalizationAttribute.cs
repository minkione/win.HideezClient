using System;

namespace HideezSafe.Modules.Localize
{
    /// <summary>
    /// Attribute for property who need localize.
    /// Used only for derived classes of LocalizedObject.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class LocalizationAttribute : Attribute
    {
    }
}
