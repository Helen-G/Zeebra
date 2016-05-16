using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Infrastructure.Crypto
{

    public sealed class CryptoProvider : ICryptoProvider
    {
        string ICryptoProvider.DefaultAlgorithm { get { return CryptoAlgorithm.Rijndael; } }

        string ICryptoProvider.Decrypt(string token, string decryptionKey, string algorithm)
        {
            return CryptoFunctions.Decrypt(token, decryptionKey, algorithm);
        }

        string ICryptoProvider.Encrypt(string data, string decryptionKey, string algorithm)
        {
           return CryptoFunctions.Encrypt(data, decryptionKey, algorithm);
        }
    }
}