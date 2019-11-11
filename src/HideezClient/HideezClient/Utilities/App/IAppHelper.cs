using System.Globalization;

namespace HideezClient.Utilities
{
    public interface IAppHelper
    {
        void ChangeCulture(CultureInfo newCulture);
        void OpenUrl(string url);
        void Shutdown();
    }
}