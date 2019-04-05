using System;

namespace HideezSafe.Models.Settings
{
    /// <summary>
    /// Attribute that specifies property as a setting for the purposes of <seealso cref="BaseSettings"/> implementation.
    /// 
    /// Properties marked with this attribute are used for reflection-based comparison in
    /// overriden Equals() and GetHashCode()
    /// </summary>
    public sealed class SettingAttribute : Attribute
    {
    }
}
