using System;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Core.Game.Extensions;
using AFT.RegoV2.Core.Game.Services;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.GameApi.Interface.Services;
using AFT.RegoV2.GameApi.Tests.Core;
using AFT.RegoV2.Infrastructure.Providers;
using Moq;
using NUnit.Framework;

namespace AFT.RegoV2.GameApi.Tests.Unit.Services
{
    [TestFixture]
    [Category("Unit")]
    public class TokenValidationProviderTests : MoqUnitTestBase
    {
        [Test]
        [ExpectedException(typeof(InvalidPlayerIpInTokenException))]
        public void ValidateToken_when_called_with_invalid_ip_throws_error()
        {
            // 1. Arrange
            // MockBehavior.Loose because we don't care about config keys
            // PrimaryTokenExpirationHours, SecondaryTokenExpirationHours
            var mConf = new Mock<IWebConfigProvider>(MockBehavior.Loose); 
            mConf
                .Setup(wcp => wcp.GetAppSettingByKey("SkipIpValidation"))
                .Returns((string)null); // same as false

            // create ITokenValidationProvider in a way that makes the order of dependencies unimportant
            ITokenValidationProvider tvp = 
                Create<TokenValidationProvider, ITokenValidationProvider>(mConf.Object);
            var token = new TokenData
            {
                PlayerIpAddress = "11.11.11.11"
            };


            // 2. Act
            // call with different IP
            tvp.ValidateToken(token, "99.99.99.99", null);

            // 3. Assert - in the ExpectedException attribute
        }

        [Test]
        [ExpectedException(typeof(ExpiredTokenException))]
        public void ValidateToken_when_called_with_expired_for_PlaceBet_operation_token_throws_error()
        {
            // 1. Arrange
            // MockBehavior.Loose because we don't care about config keys
            // SkipIpValidation
            var mConf = new Mock<IWebConfigProvider>(MockBehavior.Loose);
            mConf
                .Setup(wcp => wcp.GetAppSettingByKey("PrimaryTokenExpirationHours"))
                .Returns("24");
            mConf
                .Setup(wcp => wcp.GetAppSettingByKey("SecondaryTokenExpirationHours"))
                .Returns("48");

            // create ITokenValidationProvider in a way that makes the order of dependencies unimportant
            ITokenValidationProvider tvp = 
                Create<TokenValidationProvider, ITokenValidationProvider>(mConf.Object);

            // simulate token created 25h ago
            // PlaceBet is triggered by primary token expiration, set to 24h
            var token = new TokenData
            {
                Time = DateTime.UtcNow.AddHours(-25).ToUnixTimeSeconds() 
            };
            
            // 2. Act
            // call with different IP
            tvp.ValidateToken(token, null, typeof(PlaceBet));

            // 3. Assert - in the ExpectedException attribute
        }


        [Test]
        public void ValidateToken_when_called_with_NOT_expired_for_WinBet_operation_token_doesnot_throws_error()
        {
            // 1. Arrange
            // MockBehavior.Loose because we don't care about config keys
            // SkipIpValidation
            var mConf = new Mock<IWebConfigProvider>(MockBehavior.Loose);
            mConf
                .Setup(wcp => wcp.GetAppSettingByKey("PrimaryTokenExpirationHours"))
                .Returns("24");
            mConf
                .Setup(wcp => wcp.GetAppSettingByKey("SecondaryTokenExpirationHours"))
                .Returns("48");

            // create ITokenValidationProvider in a way that makes the order of dependencies unimportant
            ITokenValidationProvider tvp = 
                Create<TokenValidationProvider, ITokenValidationProvider>(mConf.Object);

            // simulate token created 25h ago
            // PlaceBet is triggered by primary token expiration, set to 24h
            var token = new TokenData
            {
                Time = DateTime.UtcNow.AddHours(-25).ToUnixTimeSeconds() 
            };
            
            // 2. Act
            // call with different IP
            tvp.ValidateToken(token, null, typeof(WinBet));

            // 3. Assert - no exception
        }
    }
}