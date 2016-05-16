using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface ICryptoProvider
    {
        string DefaultAlgorithm { get; }
        string Decrypt(string token, string decryptionKey, string algorithm);
        string Encrypt(string data, string decryptionKey, string algorithm);
    }
}
