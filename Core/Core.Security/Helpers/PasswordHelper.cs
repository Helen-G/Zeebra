using System;
using System.Security.Cryptography;
using System.Text;

namespace AFT.RegoV2.Core.Services.Security
{
    public static class PasswordHelper
    {
        public static string EncryptPassword(Guid userId, string password)
        {
            return StringMD5(userId.ToString().Substring(0, 4) + password);
        }

        public static string StringMD5(string data, Encoding encoding = null)
        {
            if (data == null)
            {
                return null;
            }

            if (encoding == null)
            {
                encoding = Encoding.Unicode;
            }

            return BitConverter.ToString(MD5.Create().ComputeHash(encoding.GetBytes(data)));
        }
    }
}