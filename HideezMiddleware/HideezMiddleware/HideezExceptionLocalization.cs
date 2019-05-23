using Hideez.SDK.Communication;
using HideezMiddleware.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public static class HideezExceptionLocalization
    {
        public static CultureInfo Culture
        {
            get { return ErrorCode.Culture; }
            set { ErrorCode.Culture = value; }
        }

        public static void VerifyResourcesForErrorCode(params string[] cultureNames)
        {
            foreach (string cultureName in cultureNames)
            {
                VerifyResourcesForErrorCode(new CultureInfo(cultureName));
            }
        }

        public static void VerifyResourcesForErrorCode(CultureInfo culture)
        {
            ResourceSet resourceSet = ErrorCode.ResourceManager
                .GetResourceSet(culture, true, culture.EnglishName.StartsWith("en", StringComparison.InvariantCultureIgnoreCase));

            if (resourceSet == null)
                throw new Exception($"Has not resource for culture: {culture.EnglishName}");

            var errorCodes = Enum.GetNames(typeof(HideezErrorCode));

            foreach (DictionaryEntry entry in resourceSet)
            {
                if (!errorCodes.Contains(entry.Key.ToString()))
                    throw new Exception($"Resource contains key not suported in enum HideezErrorCode. Key: {entry.Key.ToString()}, culture: {culture.EnglishName}");
            }

            foreach (var errCode in errorCodes)
            {
                string str = resourceSet.GetString(errCode);

                if (str == null)
                    throw new Exception($"HideezErrorCode is not set into resource. HideezErrorCode: {errCode}, culture: {culture.EnglishName}");
                else if (string.Empty == str)
                    throw new Exception($"Value for HideezErrorCode cannot be empty. HideezErrorCode: {errCode}, culture: {culture.EnglishName}");
                else if (str.All(Char.IsWhiteSpace))
                    throw new Exception($"Value for HideezErrorCode cannot be white space. HideezErrorCode: {errCode}, culture: {culture.EnglishName}");
            }
        }

        public static string GetErrorAsString(HideezErrorCode hideezErrorCode, CultureInfo culture = null)
        {
            return ErrorCode.ResourceManager.GetString(hideezErrorCode.ToString(), culture ?? ErrorCode.Culture);
        }

        public static string GetErrorAsString(HideezException exception, CultureInfo culture = null)
        {
            string localizedStr = ErrorCode.ResourceManager.GetString(exception.ErrorCode.ToString(), culture ?? ErrorCode.Culture);

            if (exception.Parameters != null)
            {
                return string.Format(localizedStr, exception.Parameters);
            }
            else
            {
                return localizedStr;
            }
        }
    }
}
