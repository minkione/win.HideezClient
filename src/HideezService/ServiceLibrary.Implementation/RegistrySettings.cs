using System;
using Hideez.SDK.Communication.Log;
using HideezMiddleware;

namespace ServiceLibrary.Implementation
{
    class RegistrySettings
    {
        const string _hesAddressRegistryValueName = "client_hes_address";

        public static string GetHesAddress(Logger log)
        {
            try
            {
                var registryKey = HideezClientRegistryRoot.GetRootRegistryKey();

                if (registryKey == null)
                    throw new Exception("Couldn't find Hideez Client registry key. (HKLM\\SOFTWARE\\Hideez\\Client)");

                var value = registryKey.GetValue(_hesAddressRegistryValueName);
                if (value == null)
                    throw new ArgumentNullException($"{_hesAddressRegistryValueName} value is null or empty. Please specify HES address in registry under value {_hesAddressRegistryValueName}. Key: HKLM\\SOFTWARE\\Hideez\\Client");

                if (value is string == false)
                    throw new FormatException($"{_hesAddressRegistryValueName} could not be cast to string. Check that its value has REG_SZ type");

                var address = value as string;

                if (string.IsNullOrWhiteSpace(address))
                    throw new ArgumentNullException($"{_hesAddressRegistryValueName} value is null or empty. Please specify HES address in registry under value {_hesAddressRegistryValueName}. Key: HKLM\\SOFTWARE\\Hideez\\Client");

                if (Uri.TryCreate(address, UriKind.Absolute, out Uri outUri)
                    && (outUri.Scheme == Uri.UriSchemeHttp || outUri.Scheme == Uri.UriSchemeHttps))
                {
                    return address;
                }
                else
                {
                    throw new ArgumentException($"Specified HES address: ('{address}'), " +
                        $"is not a correct absolute uri");
                }
            }
            catch (Exception ex)
            {
                log.WriteLine(ex);
            }

            return string.Empty;
        }

        public static void SetHesAddress(Logger log, string address)
        {
            try
            {
                var registryKey = HideezClientRegistryRoot.GetRootRegistryKey(true);

                if (registryKey == null)
                    throw new Exception("Couldn't find Hideez Client registry key. (HKLM\\SOFTWARE\\Hideez\\Client)");

                registryKey.SetValue(_hesAddressRegistryValueName, address);
            }
            catch (Exception ex)
            {
                log.WriteLine(ex);
            }
        }
    }
}
