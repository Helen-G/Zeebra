using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AFT.RegoV2.Infrastructure.Crypto
{
    internal sealed class CryptoSymmetric
    {
        #region Class Enumerations 

        /// <summary>
        ///     Enumeration of supported symmetric algorithms.
        /// </summary>
        public enum CryptoAlgorithm : int
        {
            CryptoAlgorithmDes,
            CryptoAlgorithmRc2,
            CryptoAlgorithmRijndael,
            CryptoAlgorithmTripleDes
        }

        /// <summary>
        ///     Enumeration of supported hash algorithms.
        ///     The Hash algorithm is used to hash the Key before encyrption/decryption.
        /// </summary>
        public enum HashAlgorithm : int
        {
            HashAlgorithmMd5,
            HashAlgorithmSha1,
            HashAlgorithmSha256,
            HashAlgorithmSha384,
            HashAlgorithmSha512,
            HashAlgorithmNone
        }

        /// <summary>
        ///     Enumeration to designate whether encryption or decryption is the
        ///     desired transformation.
        /// </summary>
        private enum CryptoMethod
        {
            CryptoMethodEncrypt,
            CryptoMethodDecrypt
        }

        #endregion

        #region Properties 

        /// <summary>
        ///     The key used for encryption/decryption.
        /// </summary>
        private byte[] _arrKey;

        /// <summary>
        ///     The initialization vector used for encryption/decryption.
        /// </summary>
        private byte[] _arrIv;

        /// <summary>
        ///     The symmetric algorithm service provider for encryption/decryption.
        /// </summary>
        private SymmetricAlgorithm _objCryptoService;

        /// <summary>
        ///     The symmetric algorithm for encryption/decryption.
        /// </summary>
        private CryptoAlgorithm _algorithmId;

        /// <summary>
        ///     The hash algorithm for hashing the key.
        /// </summary>
        private HashAlgorithm _hashId;

        #endregion

        #region Accessor Methods 

        /// <summary>
        ///     Gets or sets the encryption Key
        /// </summary>
        public byte[] Key
        {
            get { return _arrKey; }
            set
            {
                // if they set this via the accessor, make sure it's legal 
                _arrKey = GetLegalKey(value);
            }
        }

        /// <summary>
        ///     Gets or sets the Initialization Vector
        /// </summary>
        public byte[] IV
        {
            get { return _arrIv; }
            set
            {
                // if they set this via the accessor, make sure it's valid 
                _arrIv = GetValidIV(value);
            }
        }

        /// <summary>
        ///     Gets or sets the CryptoAlgorithm type
        /// </summary>
        public CryptoAlgorithm EncryptionAlgorithm
        {
            get { return _algorithmId; }
            set { _algorithmId = value; }
        }

        /// <summary>
        ///     Gets or sets the Hash Algorithm type
        /// </summary>
        public HashAlgorithm HashType
        {
            get { return _hashId; }
            set { _hashId = value; }
        }

        #endregion

        #region Constructors 

        /// <summary>
        ///     Initializes an instance of the CryptoSymmetric class.
        /// </summary>
        public CryptoSymmetric(CryptoAlgorithm crypto)
        {
            // initialize 
            Initialize(crypto, HashAlgorithm.HashAlgorithmNone);
        }

        /// <summary>
        ///     Initializes an instance of the CryptoSymmetric class.
        /// </summary>
        public CryptoSymmetric(CryptoAlgorithm crypto, string strKey)
        {
            // initialize the encryption Key and use Key as IV 
            _arrKey = GetLegalKey(strKey);
            _arrIv = GetValidIV(strKey);

            // initialize 
            Initialize(crypto, HashAlgorithm.HashAlgorithmNone);
        }

        /// <summary>
        ///     Initializes an instance of the CryptoSymmetric class.
        /// </summary>
        public CryptoSymmetric(CryptoAlgorithm crypto, string strKey, string strIV)
        {
            // initialize the encryption Key, IV 
            _arrKey = GetLegalKey(strKey);
            _arrIv = GetValidIV(strIV);

            // initialize 
            Initialize(crypto, HashAlgorithm.HashAlgorithmNone);
        }

        /// <summary>
        ///     Initializes an instance of the CryptoSymmetric class.
        /// </summary>
        public CryptoSymmetric(CryptoAlgorithm crypto, byte[] arrKey)
        {
            // initialize the encryption Key and use Key as IV 
            _arrKey = GetLegalKey(arrKey);
            _arrIv = GetValidIV(arrKey);

            // initialize 
            Initialize(crypto, HashAlgorithm.HashAlgorithmNone);
        }

        /// <summary>
        ///     Initializes an instance of the CryptoSymmetric class.
        /// </summary>
        public CryptoSymmetric(CryptoAlgorithm crypto, byte[] arrKey, byte[] arrIv)
        {
            // initialize the encryption Key, IV 
            _arrKey = GetLegalKey(arrKey);
            _arrIv = GetValidIV(arrIv);

            // initialize 
            Initialize(crypto, HashAlgorithm.HashAlgorithmNone);
        }

        /// <summary>
        ///     Initializes an instance of the CryptoSymmetric class.
        ///     The designated HashAlgorithm will be used to hash the key for encryption/decryption.
        /// </summary>
        public CryptoSymmetric(CryptoAlgorithm crypto, HashAlgorithm hash)
        {
            // initialize 
            Initialize(crypto, hash);
        }

        /// <summary>
        ///     Initializes an instance of the CryptoSymmetric class.
        ///     The designated HashAlgorithm will be used to hash the key for encryption/decryption.
        /// </summary>
        public CryptoSymmetric(CryptoAlgorithm crypto, HashAlgorithm hash, string strKey)
        {
            // initialize the encryption Key and use Key as IV 
            _arrKey = GetLegalKey(strKey);
            _arrIv = GetValidIV(strKey);

            // initialize 
            Initialize(crypto, hash);
        }

        /// <summary>
        ///     Initializes an instance of the CryptoSymmetric class.
        ///     The designated HashAlgorithm will be used to hash the key for encryption/decryption.
        /// </summary>
        public CryptoSymmetric(CryptoAlgorithm crypto, HashAlgorithm hash, string strKey, string strIv)
        {
            // initialize the encryption Key, IV 
            _arrKey = GetLegalKey(strKey);
            _arrIv = GetValidIV(strIv);

            // initialize 
            Initialize(crypto, hash);
        }

        /// <summary>
        ///     Initializes an instance of the CryptoSymmetric class.
        ///     The designated HashAlgorithm will be used to hash the key for encryption/decryption.
        /// </summary>
        public CryptoSymmetric(CryptoAlgorithm crypto, HashAlgorithm hash, byte[] arrKey)
        {
            // initialize the encryption Key and use Key as IV 
            _arrKey = GetLegalKey(arrKey);
            _arrIv = GetValidIV(arrKey);

            // initialize 
            Initialize(crypto, hash);
        }

        /// <summary>
        ///     Initializes an instance of the CryptoSymmetric class.
        ///     The designated HashAlgorithm will be used to hash the key for encryption/decryption.
        /// </summary>
        public CryptoSymmetric(CryptoAlgorithm crypto, HashAlgorithm hash, byte[] arrKey, byte[] arrIv)
        {
            // initialize the encryption Key, IV 
            _arrKey = GetLegalKey(arrKey);
            _arrIv = GetValidIV(arrIv);

            // initialize 
            Initialize(crypto, hash);
        }

        /// <summary>
        ///     Base Constructor to be used to initialize a new instance of the CryptoSymmetric class.
        /// </summary>
        private void Initialize(CryptoAlgorithm crypto, HashAlgorithm hash)
        {
            // set the crypto algorithm, obtain the proper cryptoserviceprovider 
            _algorithmId = crypto;
            switch (_algorithmId)
            {
                case CryptoAlgorithm.CryptoAlgorithmDes:
                {
                    _objCryptoService = new DESCryptoServiceProvider();
                    break;
                }
                case CryptoAlgorithm.CryptoAlgorithmRc2:
                {
                    _objCryptoService = new RC2CryptoServiceProvider();
                    break;
                }
                case CryptoAlgorithm.CryptoAlgorithmRijndael:
                {
                    _objCryptoService = new RijndaelManaged();
                    break;
                }
                case CryptoAlgorithm.CryptoAlgorithmTripleDes:
                {
                    _objCryptoService = new TripleDESCryptoServiceProvider();
                    break;
                }
            }

            // now set the hash algorithm 
            _hashId = hash;
        }

        #endregion

        #region CreateServiceProvider 

        /// <summary>
        ///     Returns the specified symmetric cryptographic service provider to enable
        ///     encryption/decryption to occur. Based on the supplied CryptoMethod,
        ///     this method will return the encryptor or decryptor.
        ///     Cipher-Block-Chaining mode is currently used for all algorithms.
        /// </summary>
        private ICryptoTransform CreateServiceProvider(CryptoMethod method)
        {
            // if we get this far without having set a key, just throw the exception and leave 
            if (_arrKey == null)
            {
                throw new CryptographicException("A key is required to " + method + " this data.");
            }

            // Pick the provider. 
            switch (_algorithmId)
            {
                case CryptoAlgorithm.CryptoAlgorithmDes:
                {
                    _objCryptoService = new DESCryptoServiceProvider();
                    _objCryptoService.Mode = CipherMode.CBC;
                    break;
                }
                case CryptoAlgorithm.CryptoAlgorithmTripleDes:
                {
                    _objCryptoService = new TripleDESCryptoServiceProvider();
                    _objCryptoService.Mode = CipherMode.CBC;
                    break;
                }
                case CryptoAlgorithm.CryptoAlgorithmRc2:
                {
                    _objCryptoService = new RC2CryptoServiceProvider();
                    _objCryptoService.Mode = CipherMode.CBC;
                    break;
                }
                case CryptoAlgorithm.CryptoAlgorithmRijndael:
                {
                    _objCryptoService = new RijndaelManaged();
                    _objCryptoService.Mode = CipherMode.CBC;
                    break;
                }
            }

            // now determine whether to send back the encryptor or decryptor 
            switch (method)
            {
                case CryptoMethod.CryptoMethodEncrypt:
                    return _objCryptoService.CreateEncryptor(_arrKey, _arrIv);

                case CryptoMethod.CryptoMethodDecrypt:
                    return _objCryptoService.CreateDecryptor(_arrKey, _arrIv);

                default:
                    throw new CryptographicException("Method '" + method + "' not supported.");
            }
        }

        #endregion

        #region Validation Methods for Key/IV 

        /// <summary>
        ///     Wrapper method to allow a string to be passed in to determine
        ///     if Key is legal for the specified symmetric algorithm.
        ///     Returns the byte array of the legal key.
        /// </summary>
        private byte[] GetLegalKey(string strKey)
        {
            // return the Key 
            return GetLegalKey(Encoding.ASCII.GetBytes(strKey));
        }

        /// <summary>
        ///     Takes a supplied byte array Key and determines if Key is legal for
        ///     the specified symmetric algorithm. If the hash algorithm has been designated,
        ///     the Key will be hashed before it is checked for validity.
        ///     Returns the byte array of the legal key.
        /// </summary>
        private byte[] GetLegalKey(byte[] arrKey)
        {
            byte[] bTemp, bHash;
            char cPadChar = ' ';

            // first determine if we are to hash the key or not 
            switch (_hashId)
            {
                case HashAlgorithm.HashAlgorithmMd5:
                    var hashMd5 = new MD5CryptoServiceProvider();
                    bHash = hashMd5.ComputeHash(arrKey);
                    // now use the hash as our key 
                    arrKey = bHash;
                    break;

                case HashAlgorithm.HashAlgorithmSha1:
                    var hashSha1 = new SHA1CryptoServiceProvider();
                    bHash = hashSha1.ComputeHash(arrKey);
                    // now use the hash as our key 
                    arrKey = bHash;
                    break;

                case HashAlgorithm.HashAlgorithmSha256:
                    SHA256 hashSha256 = new SHA256Managed();
                    bHash = hashSha256.ComputeHash(arrKey);
                    // now use the hash as our key 
                    arrKey = bHash;
                    break;

                case HashAlgorithm.HashAlgorithmSha384:
                    SHA384 hashSha384 = new SHA384Managed();
                    bHash = hashSha384.ComputeHash(arrKey);
                    // now use the hash as our key 
                    arrKey = bHash;
                    break;

                case HashAlgorithm.HashAlgorithmSha512:
                    SHA512 hashSha512 = new SHA512Managed();
                    bHash = hashSha512.ComputeHash(arrKey);
                    // now use the hash as our key 
                    arrKey = bHash;
                    break;
            }

            if (_objCryptoService.LegalKeySizes.Length > 0)
            {
                int minSize = _objCryptoService.LegalKeySizes[0].MinSize;
                int maxSize = _objCryptoService.LegalKeySizes[0].MaxSize;

                // key sizes are in bits 
                // if the key size is too small, pad the right with spaces 
                if ((arrKey.Length*8) < minSize)
                {
                    bTemp = new byte[minSize/8];

                    // first grab everything from the supplied key 
                    arrKey.CopyTo(bTemp, 0);

                    // now add spaces to the key 
                    for (int i = arrKey.Length; i < (minSize/8); i++)
                        bTemp[i] = Convert.ToByte(cPadChar);
                }
                else 
                    // if the key is too large, shorten it to fit 
                    if ((arrKey.Length*8) > maxSize)
                    {
                        bTemp = new byte[maxSize/8];

                        // now grab everything up to the cutoff point 
                        for (int j = 0; j < bTemp.Length; j++)
                            bTemp[j] = arrKey[j];
                    }
                    else
                    {
                        int iByteCount = arrKey.Length;
                        while (!_objCryptoService.ValidKeySize(iByteCount*8))
                            iByteCount++;

                        // now create a new byte array of size iByteCount 
                        bTemp = new byte[iByteCount];
                        // grab everything we can from the supplied key 
                        arrKey.CopyTo(bTemp, 0);
                        // now add spaces to the key 
                        for (int k = arrKey.Length; k < bTemp.Length; k++)
                            bTemp[k] = Convert.ToByte(cPadChar);
                    }
            }
            else
            {
                throw new CryptographicException(
                    "A Symmetric Algorithm must be selected in order to perform this operation.");
            }

            // return the byte array 
            return bTemp;
        }

        /// <summary>
        ///     Wrapper method to allow a string to be passed in to determine
        ///     if IV is valid for the specified symmetric algorithm.
        ///     Returns the byte array of the valid IV.
        /// </summary>
        private byte[] GetValidIV(string strIv)
        {
            // return the byte array 
            return GetValidIV(Encoding.ASCII.GetBytes(strIv));
        }

        /// <summary>
        ///     Takes a supplied byte array IV and determines if IV is valid for
        ///     the specified symmetric algorithm.
        ///     Returns the byte array of the valid IV.
        /// </summary>
        private byte[] GetValidIV(byte[] arrIV)
        {
            var bTemp = new byte[1];
            char cPadChar = ' ';
            int i;
            switch (_algorithmId)
            {
                case CryptoAlgorithm.CryptoAlgorithmDes:
                case CryptoAlgorithm.CryptoAlgorithmRc2:
                case CryptoAlgorithm.CryptoAlgorithmTripleDes:
                    // use 64 bit IV for DES, RC2 and TripleDES 
                    bTemp = new byte[8];
                    break;

                case CryptoAlgorithm.CryptoAlgorithmRijndael:
                    // use 128 bit IV for Rijndael 
                    bTemp = new byte[16];
                    break;
            }

            // if IV has more bytes than we need, just grab as many as we can fit in bTemp 
            // otherwise, grab them all, and pad out the remaining spots with spaces 
            if (arrIV.Length >= bTemp.Length)
            {
                for (i = 0; i < bTemp.Length; i++)
                    bTemp[i] = arrIV[i];
            }
            else
            {
                // grab what we have 
                arrIV.CopyTo(bTemp, 0);
                // now fill the rest with spaces 
                for (i = arrIV.Length; i < bTemp.Length; i++)
                    bTemp[i] = Convert.ToByte(cPadChar);
            }

            // return byte array 
            return bTemp;
        }

        #endregion

        #region String Encryption/Decryption 

        #region String Encryption Methods 

        //        public string EncryptString(string strSource) { 
        //            return EncryptString(strSource); 
        //        } 
        /// <summary>
        ///     Encrypts the supplied string and returns the ciphertext.
        ///     The CryptoSymmetric object must have its key and initialization vector defined.
        /// </summary>
        /// <summary>
        ///     Encrypts the supplied string and returns the ciphertext.
        ///     The CryptoSymmetric object will encrypt using the supplied Key for the key and initialization vector.
        /// </summary>
        public string EncryptString(string strSource, byte[] arrKey)
        {
            // set the supplied key as the encryption key 
            _arrKey = GetLegalKey(arrKey);

            // set the IV using the supplied Key 
            _arrIv = GetValidIV(arrKey);

            return EncryptString(strSource);
        }

        /// <summary>
        ///     Encrypts the supplied string and returns the ciphertext.
        ///     The CryptoSymmetric object will encrypt using the supplied Key and IV.
        /// </summary>
        public string EncryptString(string strSource, byte[] arrKey, byte[] arrIv)
        {
            // set the supplied key as the encryption key 
            _arrKey = GetLegalKey(arrKey);

            // set the IV 
            _arrIv = GetValidIV(arrIv);

            return EncryptString(strSource);
        }

        /// <summary>
        ///     Encrypts the supplied string and returns the ciphertext.
        ///     The CryptoSymmetric object will encrypt using the supplied Key for the key and initialization vector.
        /// </summary>
        public string EncryptString(string strSource, string strKey)
        {
            // set the supplied key as the encryption key 
            _arrKey = GetLegalKey(strKey);

            // set the IV using the supplied Key 
            _arrIv = GetValidIV(strKey);

            return EncryptString(strSource);
        }

        /// <summary>
        ///     Encrypts the supplied string and returns the ciphertext.
        ///     The CryptoSymmetric object will encrypt using the supplied Key and IV.
        /// </summary>
        public string EncryptString(string strSource, string strKey, string strIv)
        {
            // set the supplied key as the encryption key 
            _arrKey = GetLegalKey(strKey);

            // set the IV 
            _arrIv = GetValidIV(strIv);

            return EncryptString(strSource);
        }

        /// <summary>
        ///     The main EncryptString function. The function creates a MemoryStream, a CryptoStream
        ///     and obtains an ICryptoTransform interface. The CryptoStream then
        ///     uses the supplied source string and writes out the encrypted
        ///     text to the MemoryStream using the ICryptoTransform interface.
        /// </summary>
        public string EncryptString(string strSource)
        {
            byte[] arrInput = Encoding.ASCII.GetBytes(strSource);

            // create a MemoryStream so that the process can be done without I/O files 
            var objMs = new MemoryStream();

            // create an Encryptor 
            ICryptoTransform encrypto = CreateServiceProvider(CryptoMethod.CryptoMethodEncrypt);

            // create Crypto Stream that transforms a stream using the encryption 
            var objCs = new CryptoStream(objMs, encrypto, CryptoStreamMode.Write);

            // write out encrypted content into MemoryStream 
            objCs.Write(arrInput, 0, arrInput.Length);
            objCs.FlushFinalBlock();

            // get the output 
            byte[] arrOutput = objMs.ToArray();

            // close our streams 
            objCs.Close();
            objMs.Close();

            // convert into Base64 so that the result can be used in xml 
            return ToBase64ForUrlString(arrOutput);
        }

        #endregion

        #region String Decryption Methods 

        //        public string DecryptString(string strSource) { 
        //            return DecryptString(strSource); 
        //        } 
        /// <summary>
        ///     Decrypts the supplied string and returns the plaintext.
        ///     The CryptoSymmetric object must have its key and initialization vector defined.
        /// </summary>
        /// <summary>
        ///     Decrypts the supplied string and returns the plaintext.
        ///     The CryptoSymmetric object will decrypt using the supplied Key for the key and initialization vector.
        /// </summary>
        public string DecryptString(string strSource, byte[] arrKey)
        {
            // set the supplied key as the encryption key 
            _arrKey = GetLegalKey(arrKey);

            // set the IV using the supplied Key 
            _arrIv = GetValidIV(arrKey);

            return DecryptString(strSource);
        }

        /// <summary>
        ///     Decrypts the supplied string and returns the plaintext.
        ///     The CryptoSymmetric object will decrypt using the supplied Key and IV.
        /// </summary>
        public string DecryptString(string strSource, byte[] arrKey, byte[] arrIV)
        {
            // set the supplied key as the encryption key 
            _arrKey = GetLegalKey(arrKey);

            // set the IV 
            _arrIv = GetValidIV(arrIV);

            return DecryptString(strSource);
        }

        /// <summary>
        ///     Decrypts the supplied string and returns the plaintext.
        ///     The CryptoSymmetric object will decrypt using the supplied Key for the key and initialization vector.
        /// </summary>
        public string DecryptString(string strSource, string strKey)
        {
            // set the supplied key as the decryption key 
            _arrKey = GetLegalKey(strKey);

            // set the IV using the supplied Key 
            _arrIv = GetValidIV(strKey);

            return DecryptString(strSource);
        }

        /// <summary>
        ///     Decrypts the supplied string and returns the plaintext.
        ///     The CryptoSymmetric object will decrypt using the supplied Key and IV.
        /// </summary>
        public string DecryptString(string strSource, string strKey, string strIV)
        {
            // set the supplied key as the encryption key 
            _arrKey = GetLegalKey(strKey);

            // set the IV 
            _arrIv = GetValidIV(strIV);

            return DecryptString(strSource);
        }

        /// <summary>
        ///     The main DecryptString function. The function creates a MemoryStream, a CryptoStream
        ///     and obtains an ICryptoTransform interface. The CryptoStream then
        ///     uses the supplied source string and writes out the decrypted
        ///     text to the MemoryStream using the ICryptoTransform interface.
        /// </summary>
        public string DecryptString(string strSource)
        {
            // convert from Base64 to binary 
            //byte[] arrInput = System.Convert.FromBase64String(strSource); 
            byte[] arrInput = FromBase64ForUrlString(strSource);

            // create a MemoryStream with the input 
            var objMs = new MemoryStream(arrInput, 0, arrInput.Length);

            // create a Decryptor 
            ICryptoTransform decrypto = CreateServiceProvider(CryptoMethod.CryptoMethodDecrypt);

            // create Crypto Stream that transforms the stream using the decryption 
            var objCs = new CryptoStream(objMs, decrypto, CryptoStreamMode.Read);

            // allocate the buffer long enough to hold ciphertext (plaintext is never longer than ciphertext) 
            var arrOutput = new byte[arrInput.Length];

            // Start decrypting. 
            int intDecryptedByteCount = objCs.Read(arrOutput, 0, arrOutput.Length);

            // Close both streams. 
            objCs.Close();
            objMs.Close();

            // Convert decrypted data into a string. 
            return Encoding.ASCII.GetString(arrOutput, 0, intDecryptedByteCount);
        }

        #endregion

        #endregion

        private static string ToBase64ForUrlString(byte[] input)
        {
            var result = new StringBuilder(Convert.ToBase64String(input).TrimEnd('='));
            result.Replace('+', '-');
            result.Replace('/', '_');
            return result.ToString();
        }

        private static byte[] FromBase64ForUrlString(string base64ForUrlInput)
        {
            int padChars = (base64ForUrlInput.Length%4) == 0 ? 0 : (4 - (base64ForUrlInput.Length%4));
            var result = new StringBuilder(base64ForUrlInput, base64ForUrlInput.Length + padChars);
            result.Append(String.Empty.PadRight(padChars, '='));
            result.Replace('-', '+');
            result.Replace('_', '/');
            return Convert.FromBase64String(result.ToString());
        }
    }
}