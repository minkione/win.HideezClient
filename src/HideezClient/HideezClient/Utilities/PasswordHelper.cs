using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HideezClient.Utilities
{
    [Flags]
    public enum PasswordRestrictions
    {
        None = 0,       // 0b_0000_0000
        Numbers = 1,    // 0b_0000_0001
        UpperChar = 2,  // 0b_0000_0010
        LowerChar = 4,  // 0b_0000_0100
        Symbols = 8,    // 0b_0000_1000 
        MinMax = 16,    // 0b_0001_0000
    }

    class PasswordHelper
    {
        private Regex hasNumber = new Regex(@"[0-9]+");
        private Regex hasUpperChar = new Regex(@"[A-Z]+");
        private Regex hasLowerChar = new Regex(@"[a-z]+");
        private Regex hasMinMaxChars;
        private Regex hasSymbols;
        private int min = 1;
        private int max = 8;
        private string symbols = @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]";

        public PasswordHelper()
        {
            hasNumber = new Regex(@"[0-9]+");
            hasUpperChar = new Regex(@"[A-Z]+");
            hasLowerChar = new Regex(@"[a-z]+");
            hasMinMaxChars = new Regex($@".{{{min},{max}}}");
            hasSymbols = new Regex(symbols);
        }

        public int Max
        {
            get { return max; }
            set
            {
                max = value;
                hasMinMaxChars = new Regex($@".{{{min},{max}}}");
            }
        }
        public int Min
        {
            get { return min; }
            set
            {
                min = value;
                hasMinMaxChars = new Regex($@".{{{min},{max}}}");
            }
        }
        public string Symbols
        {
            get { return symbols; }
            set
            {
                symbols = value;
                hasSymbols = new Regex(symbols);
            }
        }

        public PasswordRestrictions ShouldContains { get; set; }
        public PasswordRestrictions CanContains { get; set; }

        private bool IsEnabledShould(PasswordRestrictions restrictions)
        {
            return (ShouldContains & restrictions) == restrictions;
        }

        private bool IsEnabledCan(PasswordRestrictions restrictions)
        {
            return (CanContains & restrictions) == restrictions;
        }

        public bool CanInput(string character)
        {
            return CanContains == PasswordRestrictions.None
                 ||(IsEnabledCan(PasswordRestrictions.LowerChar) && hasLowerChar.IsMatch(character)
                 || IsEnabledCan(PasswordRestrictions.UpperChar) && hasUpperChar.IsMatch(character)
                 || IsEnabledCan(PasswordRestrictions.MinMax) && hasMinMaxChars.IsMatch(character)
                 || IsEnabledCan(PasswordRestrictions.Numbers) && hasNumber.IsMatch(character)
                 || IsEnabledCan(PasswordRestrictions.Symbols) && hasSymbols.IsMatch(character)
            );
        }

        public bool ValidatePassword(string password, out string ErrorMessage)
        {
            var input = password;
            ErrorMessage = string.Empty;
            bool isValid = false;

            if (string.IsNullOrWhiteSpace(input))
            {
                ErrorMessage = "Password should not be empty";
            }
            else if (ShouldContains == PasswordRestrictions.None)
            {
                isValid = true;
            }
            else if (IsEnabledShould(PasswordRestrictions.LowerChar) && !hasLowerChar.IsMatch(input))
            {
                ErrorMessage = "Password should contain At least one lower case letter";
            }
            else if (IsEnabledShould(PasswordRestrictions.UpperChar) && !hasUpperChar.IsMatch(input))
            {
                ErrorMessage = "Password should contain At least one upper case letter";
            }
            else if (IsEnabledShould(PasswordRestrictions.MinMax) && !hasMinMaxChars.IsMatch(input))
            {
                ErrorMessage = "Password should not be less than or greater than 12 characters";
            }
            else if (IsEnabledShould(PasswordRestrictions.Numbers) && !hasNumber.IsMatch(input))
            {
                ErrorMessage = "Password should contain At least one numeric value";
            }
            else if (IsEnabledShould(PasswordRestrictions.Symbols) && !hasSymbols.IsMatch(input))
            {
                ErrorMessage = "Password should contain At least one special case characters";
            }
            else
            {
                isValid = true;
            }

            return isValid;
        }
    }
}
