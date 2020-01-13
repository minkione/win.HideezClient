using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace HideezClient.Utilities
{
    [Flags]
    public enum PasswordChars
    {
        None = 0,
        UpperCase = 1,
        LowerCase = 2,
        Numeric = 4,
        Special = 8,
    }

    public class PasswordGenerator
    {
        private static readonly int maxCountIdenticalCharacters = 2;

        private static readonly int defaultMinLenghy = 8;
        private static readonly int defaultMaxLenght = 32;

        private static readonly char[] upperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] lowerCaseChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly char[] numericChars = "0123456789".ToCharArray();
        private static readonly char[] specialChars = "!@#$%^&*()_+|}{?><".ToCharArray();


        public static string Generate()
        {
            return Generate(defaultMinLenghy, defaultMaxLenght);
        }

        public static string Generate(PasswordChars includeChars)
        {
            return Generate(defaultMinLenghy, defaultMaxLenght, includeChars);
        }

        public static string Generate(int length)
        {
            return Generate(length, length);
        }

        public static string Generate(int length, PasswordChars includeChars)
        {
            return Generate(length, length, includeChars);
        }

        public static string Generate(int minLength, int maxLength)
        {
            return Generate(minLength, maxLength, PasswordChars.UpperCase | PasswordChars.LowerCase | PasswordChars.Numeric | PasswordChars.Special);
        }

        public static string Generate(int minLength, int maxLength, PasswordChars includeChars)
        {
            if (minLength <= 0 || maxLength <= 0 || minLength > maxLength || includeChars == PasswordChars.None)
                throw new ArgumentOutOfRangeException("Input parameters are not valid.");

            IList<char[]> charGroups = new List<char[]>();

            if ((includeChars & PasswordChars.UpperCase) == PasswordChars.UpperCase)
            {
                charGroups.Add(upperCaseChars);
            }

            if ((includeChars & PasswordChars.LowerCase) == PasswordChars.LowerCase)
            {
                charGroups.Add(lowerCaseChars);
            }

            if ((includeChars & PasswordChars.Numeric) == PasswordChars.Numeric)
            {
                charGroups.Add(numericChars);
            }

            if ((includeChars & PasswordChars.Special) == PasswordChars.Special)
            {
                charGroups.Add(specialChars);
            }

            Random random = GetRandom();
            int passwordLength;

            if (minLength < maxLength)
            {
                passwordLength = random.Next(minLength, maxLength + 1);
            }
            else
            {
                passwordLength = minLength;
            }

            char[] allCharacters = charGroups.SelectMany(chars => chars).ToArray();
            var sb = new StringBuilder();
            // Generate random string
            for (var i = 0; i < passwordLength; i++)
            {
                var randomIndex = random.Next(allCharacters.Length);
                char c = allCharacters[randomIndex];
                sb.Append(c);

                // Ensure that generated string has not more than 2 identical characters in a row (e.g., 111 not allowed)
                while (!IsRepeatChars(sb, c, 0, sb.Length, maxCountIdenticalCharacters))
                {
                    randomIndex = random.Next(allCharacters.Length);
                    c = allCharacters[randomIndex];
                    sb[i] = c;
                }
            }

            // Populate generated string with required characters
            var usedIndexes = new List<int>();
            for (var i = 0; i < charGroups.Count; i++)
            {
                var symbols = charGroups[i];

                // Generate random index not used in this loop before
                var randomResIndex = random.Next(passwordLength);
                while (usedIndexes.Contains(randomResIndex))
                {
                    randomResIndex = random.Next(passwordLength);
                }
                usedIndexes.Add(randomResIndex);

                char c;
                do
                {
                    var randomSymbolIndex = random.Next(symbols.Length);
                    c = symbols[randomSymbolIndex];
                    sb[randomResIndex] = c;
                } while (!IsRepeatChars(sb, c, 0, sb.Length, maxCountIdenticalCharacters));
            }

            return sb.ToString();
        }

        private static bool IsRepeatChars(StringBuilder sb, char c, int startIndex, int lenght, int countRepeat)
        {
            for (int i = startIndex + lenght - 1; i > 0; i--)
            {
                for (int repeatIndex = i; sb[repeatIndex] == c && repeatIndex > 0; repeatIndex--)
                {
                    if (countRepeat == i - repeatIndex)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static Random GetRandom()
        {
            byte[] randomBytes = new byte[4];

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);

            int seed = BitConverter.ToInt32(randomBytes, 0);

            Random random = new Random(seed);

            return random;
        }
    }
}
