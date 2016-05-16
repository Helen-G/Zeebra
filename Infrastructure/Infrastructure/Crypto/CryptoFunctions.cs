using System;
using System.Security.Cryptography;
using System.Text;
using AFT.RegoV2.Core.Common.Data;

namespace AFT.RegoV2.Infrastructure.Crypto
{
    internal static class CryptoFunctions
    {
        // perform a one-way hash on a text string
        public static string Hash(string strText, string strHashMethod)
        {
            string strReturn = string.Empty;
            byte[] arrKey = Encoding.UTF8.GetBytes(strText);
            byte[] arrHash = null;
            bool blnComputeFromArray = true;
            switch (GetHash(strHashMethod))
            {
                case CryptoSymmetric.HashAlgorithm.HashAlgorithmMd5:
                    var hashMd5 = new MD5CryptoServiceProvider();
                    arrHash = hashMd5.ComputeHash(arrKey);
                    break;
                case CryptoSymmetric.HashAlgorithm.HashAlgorithmSha1:
                    var hashSha1 = new SHA1CryptoServiceProvider();
                    arrHash = hashSha1.ComputeHash(arrKey);
                    break;
                case CryptoSymmetric.HashAlgorithm.HashAlgorithmSha256:
                    SHA256 hashSha256 = new SHA256Managed();
                    arrHash = hashSha256.ComputeHash(arrKey);
                    break;
                case CryptoSymmetric.HashAlgorithm.HashAlgorithmSha384:
                    SHA384 hashSha384 = new SHA384Managed();
                    arrHash = hashSha384.ComputeHash(arrKey);
                    break;
                case CryptoSymmetric.HashAlgorithm.HashAlgorithmSha512:
                    SHA512 hashSha512 = new SHA512Managed();
                    arrHash = hashSha512.ComputeHash(arrKey);
                    break;
                case CryptoSymmetric.HashAlgorithm.HashAlgorithmNone:
                    strReturn = strText;
                    blnComputeFromArray = false;
                    break;
                default:
                    strReturn = string.Empty;
                    blnComputeFromArray = false;
                    break;
            }
            if (blnComputeFromArray)
            {
                foreach (byte b in arrHash)
                {
                    strReturn += String.Format("{0:x2}", b);
                }
            }
            return strReturn;
        }

        private const string DefaultEncryptionKey = "C7C7C6B9-E75C-43A8-B986-D82F41E23E2C";

        public static string Encrypt(string strPlainText)
        {
            return Encrypt(strPlainText, DefaultEncryptionKey);
        }

        public static string Encrypt(string strPlainText, string strKey)
        {
            var objCs = new CryptoSymmetric(CryptoSymmetric.CryptoAlgorithm.CryptoAlgorithmTripleDes);
            return objCs.EncryptString(strPlainText, strKey);
        }

        public static string Encrypt(string strPlainText, string strKey, string strAlgorithm)
        {
            var objCs = new CryptoSymmetric(GetAlgorithm(strAlgorithm));
            return objCs.EncryptString(strPlainText, strKey);
        }

        public static string Encrypt(string strPlainText, string strKey, string strAlgorithm, string strHash)
        {
            var objCs = new CryptoSymmetric(GetAlgorithm(strAlgorithm), GetHash(strHash));
            return objCs.EncryptString(strPlainText, strKey);
        }

        public static string Decrypt(string strCipherText)
        {
            return Decrypt(strCipherText, DefaultEncryptionKey);
        }

        public static string Decrypt(string strCipherText, string strKey)
        {
            var objCs = new CryptoSymmetric(CryptoSymmetric.CryptoAlgorithm.CryptoAlgorithmDes);
            return objCs.DecryptString(strCipherText, strKey);
        }

        public static string Decrypt(string strCipherText, string strKey, string strAlgorithm)
        {
            var objCs = new CryptoSymmetric(GetAlgorithm(strAlgorithm));
            return objCs.DecryptString(strCipherText, strKey);
        }

        public static string Decrypt(string strCipherText, string strKey, string strAlgorithm, string strHash)
        {
            var objCs = new CryptoSymmetric(GetAlgorithm(strAlgorithm), GetHash(strHash));
            return objCs.DecryptString(strCipherText, strKey);
        }

        private static CryptoSymmetric.CryptoAlgorithm GetAlgorithm(string strAlgorithm)
        {
            switch (strAlgorithm.ToUpper())
            {
                case CryptoAlgorithm.Des:
                    return CryptoSymmetric.CryptoAlgorithm.CryptoAlgorithmDes;
                case CryptoAlgorithm.Rc2:
                    return CryptoSymmetric.CryptoAlgorithm.CryptoAlgorithmRc2;
                case CryptoAlgorithm.Rijndael:
                    return CryptoSymmetric.CryptoAlgorithm.CryptoAlgorithmRijndael;
                case CryptoAlgorithm.TripleDes:
                    return CryptoSymmetric.CryptoAlgorithm.CryptoAlgorithmTripleDes;
                default:
                    return CryptoSymmetric.CryptoAlgorithm.CryptoAlgorithmRijndael;
            }
        }

        private static CryptoSymmetric.HashAlgorithm GetHash(string strHash)
        {
            switch (strHash.ToUpper())
            {
                case CryptoHash.None:
                    return CryptoSymmetric.HashAlgorithm.HashAlgorithmNone;
                case CryptoHash.Md5:
                    return CryptoSymmetric.HashAlgorithm.HashAlgorithmMd5;
                case CryptoHash.Sha1:
                    return CryptoSymmetric.HashAlgorithm.HashAlgorithmSha1;
                case CryptoHash.Sha256:
                    return CryptoSymmetric.HashAlgorithm.HashAlgorithmSha256;
                case CryptoHash.Sha384:
                    return CryptoSymmetric.HashAlgorithm.HashAlgorithmSha384;
                case CryptoHash.Sha512:
                    return CryptoSymmetric.HashAlgorithm.HashAlgorithmSha512;
                default:
                    return CryptoSymmetric.HashAlgorithm.HashAlgorithmMd5;
            }
        }
    }
}