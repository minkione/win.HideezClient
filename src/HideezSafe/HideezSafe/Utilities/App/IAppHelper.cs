using System.Globalization;

namespace HideezSafe.Utilities
{
    interface IAppHelper
    {
        void ChangeCulture(CultureInfo newCulture);
        void OpenUrl(string url);
        void Shutdown();
    }
}