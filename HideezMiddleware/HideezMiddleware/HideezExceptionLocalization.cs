using Hideez.SDK.Communication;
using HideezMiddleware.Resources;
using NLog;
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

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public static CultureInfo Culture
        {
            get { return ErrorCode.Culture; }
            set { ErrorCode.Culture = value; }
        }


        public static bool VerifyResourcesForErrorCode(params string[] cultureNames)
        {
            bool isValid = true;
            foreach (string cultureName in cultureNames)
            {
                isValid |= VerifyResourcesForErrorCode(new CultureInfo(cultureName));
            }

            return isValid;
        }

        public static bool VerifyResourcesForErrorCode(CultureInfo culture)
        {
            bool isValid = true;
            // English culture read from default resource
            ResourceSet resourceSet = ErrorCode.ResourceManager
                .GetResourceSet(culture, true, culture.EnglishName.StartsWith("en", StringComparison.InvariantCultureIgnoreCase));

            if (resourceSet == null)
            {
                isValid = false;
                log.Error($"Has no resource for culture: {culture.EnglishName}");
            }

            var errorCodes = Enum.GetNames(typeof(HideezErrorCode));

            foreach (DictionaryEntry entry in resourceSet)
            {
                if (!errorCodes.Contains(entry.Key.ToString()))
                {
                    isValid = false;
                    log.Error($"Resource contains key not suported in enum HideezErrorCode. Key: {entry.Key.ToString()}, culture: {culture.EnglishName}");
                }
            }

            foreach (var errCode in errorCodes)
            {
                string str = resourceSet.GetString(errCode);

                if (str == null)
                {
                    isValid = false;
                    log.Error($"HideezErrorCode is not set into resource. HideezErrorCode: {errCode}, culture: {culture.EnglishName}");
                }
                else if (string.IsNullOrWhiteSpace(str))
                {
                    isValid = false;
                    log.Error($"Value for HideezErrorCode cannot be empty or white space. HideezErrorCode: {errCode}, culture: {culture.EnglishName}");
                }
            }

            return isValid;
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
