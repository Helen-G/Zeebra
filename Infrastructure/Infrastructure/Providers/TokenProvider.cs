using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Exceptions;


namespace AFT.RegoV2.Infrastructure.Providers
{
    public interface ITokenProvider
    {
        TokenData Decrypt(string tokenString);
        string Encrypt(TokenData data);
    }
    public sealed class TokenProvider : ITokenProvider
    {
        private readonly IWebConfigProvider _config;
        private readonly ICryptoProvider _cryptoProvider;
        private readonly IJsonSerializationProvider _jsonSerialization;
        private MachineDecryptionInfo _info;
        private static readonly IDictionary<string, string> KnownAlgorithms = 
            new ReadOnlyDictionary<string, string>(
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        {"RIJNDAEL", CryptoAlgorithm.Rijndael},
                        {"AES", CryptoAlgorithm.Rijndael},
                        {"RC2", CryptoAlgorithm.Rc2},
                        {"DES", CryptoAlgorithm.Des},
                        {"TRIPLEDES", CryptoAlgorithm.TripleDes}
                    });

        public TokenProvider(
            IWebConfigProvider config,
            ICryptoProvider cryptoProvider,
            IJsonSerializationProvider jsonSerialization)
        {
            _config = config;
            _cryptoProvider = cryptoProvider;
            _jsonSerialization = jsonSerialization;
        }

        TokenData ITokenProvider.Decrypt(string tokenString)
        {
            try
            {
                if (String.IsNullOrEmpty(tokenString))
                {
                    throw new ArgumentException("Missing token string");      
                }

                EnsureMachineDecryptionInfo();

                var tokenDecrypted = _cryptoProvider.Decrypt(tokenString, _info.DecryptionKey, _info.DecryptionAlgorithm);

                var data = _jsonSerialization.DeserializeFromString<TokenData>(tokenDecrypted);

                if (data == null)
                {
                    throw new InvalidTokenException(
                        String.Format("DeserializeFromString returned null. Token string: {0}", tokenString));
                }

                return data;
            }
            catch (InvalidTokenException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new InvalidTokenException(
                    String.Format("Token parsing general exception. {0}. Token string: {1} ", ex.Message, tokenString));
            }
        }
        string ITokenProvider.Encrypt(TokenData data)
        {
            EnsureMachineDecryptionInfo();

            var json = _jsonSerialization.SerializeToString(data);

            return _cryptoProvider.Encrypt(json, _info.DecryptionKey, _info.DecryptionAlgorithm);
        }
        private bool TryFindAlgorithm(string decryptionAlgorithm, out string algorithm)
        {
            if (KnownAlgorithms.TryGetValue(decryptionAlgorithm, out algorithm)) return true;
            algorithm = _cryptoProvider.DefaultAlgorithm;
            return false;
        }
        private void EnsureMachineDecryptionInfo()
        {
            if (_info == null)
            {
                _info = _config.GetMachineDecryptionInfo();
                string algorithm;
                TryFindAlgorithm(_info.DecryptionAlgorithm, out algorithm);
                _info.DecryptionAlgorithm = algorithm;
            }
        }
    }
}