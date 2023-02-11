using BingoVintage.Data;
using System.Security.Claims;

namespace BingoVintage.ExtentionMethods
{
    public class UsrMethod
    {
        //Random Token Generator.
        public string TokenGenerator(byte length)
        {
            string token = "";
            for (byte i = 0; i < length; i++)
            {
                char t = GetChar();
                token += t.ToString();
            }
            return token;
        }
        private static char GetChar() 
        {
            string chars = "@#~$€%123456ABCDEFGHIJKLMNÑOPQRSTUVWXYZ<+";
            int randomChar = new Random().Next(0, chars.Length);
            return chars[randomChar];
        }
        public int CompareIdUsrCookieToDB(ClaimsPrincipal claims)
        {
            ClaimsIdentity? i = claims.Identities.FirstOrDefault();
            List<Claim> c = i!.Claims.ToList();
            var nameIdentifierID = System.Convert.ToString(c[0]);
            string num = nameIdentifierID!.Substring(70);
            int IdUsr = num.NormalizeToInt();
            var idFromDb = new UsrData().GetUsrId(IdUsr);
            return idFromDb;
        }
    }
}