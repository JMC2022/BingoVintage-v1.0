using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BingoVintage.ExtentionMethods
{
    public static class StringMethod
    {
        //Email Validator.
        public static bool IsEmailValid(this string s)
        {
            //Remove white spaces from string.
            var trimStr = s.Trim();
            //String end with "." ?
            if (trimStr.EndsWith("."))
            {
                return false;
            }

            //Verify string - "Valid attrib".
            try
            {
                if (!new EmailAddressAttribute().IsValid(trimStr))
                {
                    return false;
                }
                //Domain and Struct of an Email
                if (Regex.IsMatch(trimStr, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnorePatternWhitespace |
                                              RegexOptions.IgnoreCase))
                { return true; }
                else { return false; }
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        //Just for Claims Cookies.
        public static int NormalizeToInt(this string s)
        {
            int v = int.Parse(s);
            return v;
        }
    }
}