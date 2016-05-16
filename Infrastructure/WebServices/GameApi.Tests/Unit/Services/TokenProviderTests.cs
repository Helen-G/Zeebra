using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.GameApi.Tests.Core;
using AFT.RegoV2.Infrastructure.Providers;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AFT.RegoV2.GameApi.Tests.Unit.Services
{
    [TestFixture]
    [Category("Unit")]
    public class TokenProviderTests : MoqUnitTestBase
    {
        [Test]
        public void Can_Encrypt_Token()
        {
            // 1. Arrange
            var mdi = new MachineDecryptionInfo
            {
                DecryptionAlgorithm = "RIJNDAEL",  
                DecryptionKey = "MachineKey"
            };
            var mConf = new Mock<IWebConfigProvider>(MockBehavior.Strict);
            mConf
                .Setup(wcp => wcp.GetMachineDecryptionInfo())
                .Returns(mdi);

            var tokenData = new TokenData();
            const string jsonData = "{demo:1}";
            var mJson = new Mock<IJsonSerializationProvider>(MockBehavior.Strict);
            mJson
                .Setup(jsp => jsp.SerializeToString(tokenData))
                .Returns(jsonData);

            const string encrypted = "123456789";
            var mCryp = new Mock<ICryptoProvider>(MockBehavior.Strict);
            mCryp
                .Setup(cp => cp.Encrypt(jsonData, mdi.DecryptionKey, mdi.DecryptionAlgorithm))
                .Returns(encrypted);

            // create TokenProvider in a way that makes the order of dependencies unimportant
            ITokenProvider tp = Create<TokenProvider, ITokenProvider>(mConf.Object, mJson.Object, mCryp.Object);
            
            // 2. Act
            var token = tp.Encrypt(tokenData);

            // 3. Assert 
            // assert that we use expected dependencies with expected parameters expected number of times
            mConf.Verify(m => m.GetMachineDecryptionInfo(), Times.Once());
            mJson.Verify(m => m.SerializeToString(tokenData), Times.Once());
            mCryp.Verify(m => m.Encrypt(jsonData, mdi.DecryptionKey, mdi.DecryptionAlgorithm), Times.Once());
            // assert the result is as expected (whatever ICryptoProvider returns)
            token.Should().Be(encrypted);
        }

        [Test]
        public void Can_Decrypt_Token()
        {
            // 1. Arrange
            var mdi = new MachineDecryptionInfo
            {
                DecryptionAlgorithm = "RIJNDAEL",
                DecryptionKey = "MachineKey"
            };
            var mConf = new Mock<IWebConfigProvider>(MockBehavior.Strict);
            mConf
                .Setup(wcp => wcp.GetMachineDecryptionInfo())
                .Returns(mdi);

            const string encrypted = "123456789";
            const string jsonData = "{demo:1}";
            var mCryp = new Mock<ICryptoProvider>(MockBehavior.Strict);
            mCryp
                .Setup(cp => cp.Decrypt(encrypted, mdi.DecryptionKey, mdi.DecryptionAlgorithm))
                .Returns(jsonData);

            var tokenData = new TokenData();
            
            var mJson = new Mock<IJsonSerializationProvider>(MockBehavior.Strict);
            mJson
                .Setup(jsp => jsp.DeserializeFromString<TokenData>(jsonData))
                .Returns(tokenData);

            // create TokenProvider in a way that makes the order of dependencies unimportant
            ITokenProvider tp = Create<TokenProvider, ITokenProvider>(mConf.Object, mJson.Object, mCryp.Object);

            // 2. Act
            var decrypted = tp.Decrypt(encrypted);

            // 3. Assert 
            // assert that we use expected dependencies with expected parameters expected number of times
            mConf.Verify(m => m.GetMachineDecryptionInfo(), Times.Once());
            mCryp.Verify(m => m.Decrypt(encrypted, mdi.DecryptionKey, mdi.DecryptionAlgorithm), Times.Once());
            mJson.Verify(m => m.DeserializeFromString<TokenData>(jsonData), Times.Once());

            // assert the result is as expected (whatever IJsonSerializationProvider returns)
            decrypted.Should().Be(tokenData);
        }
    }
}