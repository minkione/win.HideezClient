using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace HideezMiddleware.Utils
{
    // see https://docs.microsoft.com/en-us/uwp/api/windows.security.cryptography.core.keyderivationalgorithmprovider?view=winrt-19041
    public class KdfKeyProvider
    {
        public static byte[] CreateKDFKey(byte[] password, uint targetKeySize, byte[] salt)
        {
            string strAlgName = KeyDerivationAlgorithmNames.Pbkdf2Sha256;
            uint iterationCount = 10000;
            IBuffer buffKeyMaterial = DeriveKeyMaterialPbkdf(password, strAlgName, salt, targetKeySize, iterationCount);

            //IBuffer keyBuff = key.ExportPublicKey();
            CryptographicBuffer.CopyToByteArray(buffKeyMaterial, out byte[] result);

            return result;
        }

        static IBuffer DeriveKeyMaterialPbkdf(byte[] strSecret, string strAlgName, byte[] salt, uint targetKeySize, uint iterationCount)
        {
            // Open the specified algorithm.
            KeyDerivationAlgorithmProvider objKdfProv = KeyDerivationAlgorithmProvider.OpenAlgorithm(strAlgName);

            // Create a buffer that contains the secret used during derivation.
            IBuffer buffSecret = CryptographicBuffer.CreateFromByteArray(strSecret);

            // Create an IBuffer for salt value.
            IBuffer buffSalt = CryptographicBuffer.CreateFromByteArray(salt);

            // Create the derivation parameters.
            KeyDerivationParameters pbkdf2Params = KeyDerivationParameters.BuildForPbkdf2(buffSalt, iterationCount);

            // Create a key from the secret value.
            CryptographicKey keyOriginal = objKdfProv.CreateKey(buffSecret);

            // Derive a key based on the original key and the derivation parameters.
            IBuffer keyMaterial = CryptographicEngine.DeriveKeyMaterial(
                keyOriginal,
                pbkdf2Params,
                targetKeySize);

            // return the KDF key material.
            return keyMaterial;
        }
    }
}
