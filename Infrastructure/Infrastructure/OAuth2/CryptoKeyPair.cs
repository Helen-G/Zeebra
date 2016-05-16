using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace AFT.RegoV2.Infrastructure.OAuth2
{
    public class CryptoKeyPair : ICryptoKeyPair
    {

        public static ICryptoKeyPair LoadCertificate(string certificateFilePath, string certificatePassword)
        {
            var fileInfo = new FileInfo(certificateFilePath);
            return (new CryptoKeyPair(fileInfo, certificatePassword));
        }

        public CryptoKeyPair(FileSystemInfo certificateFileInfo, string certificatePassword)
        {
            var cert = new X509Certificate2(certificateFileInfo.FullName, certificatePassword, X509KeyStorageFlags.MachineKeySet);
            _publicKeyProvider = cert.PublicKey.Key as RSACryptoServiceProvider;
            _secretKeyProvider = cert.PrivateKey as RSACryptoServiceProvider;
        }

        public CryptoKeyPair(RSAParameters publicKey, RSAParameters secretKey)
        {
            _publicKeyProvider = new RSACryptoServiceProvider();
            _secretKeyProvider = new RSACryptoServiceProvider();
            _publicKeyProvider.ImportParameters(publicKey);
            _secretKeyProvider.ImportParameters(secretKey);
        }

        private readonly RSACryptoServiceProvider _publicKeyProvider;
        private readonly RSACryptoServiceProvider _secretKeyProvider;

        public RSACryptoServiceProvider PublicSigningKey
        {
            get { return (_publicKeyProvider); }
        }

        public RSACryptoServiceProvider PrivateEncryptionKey
        {
            get { return (_secretKeyProvider); }
        }
    }
}
