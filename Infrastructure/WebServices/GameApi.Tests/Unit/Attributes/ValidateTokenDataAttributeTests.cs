using System;
using System.Web.Http.Controllers;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Core.Game.Services;
using AFT.RegoV2.GameApi.Interface.Attributes;
using AFT.RegoV2.GameApi.Interface.Classes;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.GameApi.Interface.Services;
using AFT.RegoV2.GameApi.Tests.Core;
using AFT.RegoV2.Infrastructure.Providers;
using Moq;
using NUnit.Framework;

namespace AFT.RegoV2.GameApi.Tests.Unit.Attributes
{
    [TestFixture]
    [Category("Unit")]
    public class ValidateTokenDataAttributeTests : MoqUnitTestBase
    {
        [Test]
        public void If_no_token_error_no_response_is_set_and_token_data_is_set()
        {
            // 1. Arrange
            var attr = new ValidateTokenDataAttribute();
            var context = new HttpActionContext
            {
                ActionDescriptor = new ReflectedHttpActionDescriptor()
            };
            const string tokenString = "token";
            var request = new ValidateToken
            {
                AuthToken = tokenString
            };
            context.ActionArguments.Add("request", request);

            var token = new TokenData {};
            var mtp = new Mock<ITokenProvider>(MockBehavior.Strict);
            mtp
                .Setup(tp => tp.Decrypt(tokenString))
                .Returns(token);
            attr.TokenProvider = mtp.Object;

            var mtvp = new Mock<ITokenValidationProvider>(MockBehavior.Strict);
            mtvp
                .Setup(tvp => tvp.ValidateToken(token, It.IsAny<string>(), typeof(ValidateToken))); // no error
            attr.TokenValidation = mtvp.Object;

            var mJson = new Mock<IJsonSerializationProvider>(MockBehavior.Strict); 
            mJson
                .Setup(j => j.SerializeToString(It.IsAny<GameApiResponseBase>()))
                .Returns<GameApiResponseBase>(o => o.ErrorDescription);
            attr.Json = mJson.Object;

            // 2. Act
            attr.OnActionExecuting(context);

            // 3. Assert
            Assert.That(context.Response, Is.Null);
//            Assert.That(request.TokenData, Is.EqualTo(token));
            mtp.Verify(tp => tp.Decrypt(tokenString), Times.Once());
            mtvp.Verify(tvp => tvp.ValidateToken(token, It.IsAny<string>(), typeof(ValidateToken)), Times.Once());
        }
        [Test]
        public void On_token_error_response_is_set()
        {
            // 1. Arrange
            var attr = new ValidateTokenDataAttribute();
            var context = new HttpActionContext
            {
                ActionDescriptor = new ReflectedHttpActionDescriptor()
            };
            const string tokenString = "token";
            context.ActionArguments.Add("request", new ValidateToken
            {
                AuthToken = tokenString
            });

            var token = new TokenData {};
            var mtp = new Mock<ITokenProvider>(MockBehavior.Strict);
            mtp
                .Setup(tp => tp.Decrypt(tokenString))
                .Returns(token);
            attr.TokenProvider = mtp.Object;

            var errorMessage = Guid.NewGuid().ToString();
            var exception = new InvalidTokenException(errorMessage);
            
            var mtvp = new Mock<ITokenValidationProvider>(MockBehavior.Strict);
            mtvp
                .Setup(tvp => tvp.ValidateToken(token, It.IsAny<string>(), typeof(ValidateToken)))
                .Throws(exception); // error
            attr.TokenValidation = mtvp.Object;

            // ReSharper disable once RedundantAssignment
            string description = errorMessage;
            var mErr = new Mock<IErrorManager>(MockBehavior.Strict); 
            mErr
                .Setup(em => em.GetErrorCodeByException(exception, out description))
                .Returns(GameApiErrorCode.SystemError);
            attr.ErrorManager = mErr.Object;
            
            var mLog = new Mock<IGameProviderLog>(MockBehavior.Strict); 
            mLog
                .Setup(l => l.LogError(It.IsAny<string>(), exception));
            attr.Log = mLog.Object;

            var mJson = new Mock<IJsonSerializationProvider>(MockBehavior.Strict); 
            mJson
                .Setup(j => j.SerializeToString(It.IsAny<GameApiResponseBase>()))
                .Returns<GameApiResponseBase>(o => o.ErrorDescription);
            attr.Json = mJson.Object;

            // 2. Act
            attr.OnActionExecuting(context);

            // 3. Assert
            Assert.That(context.Response.Content.ReadAsStringAsync().Result, Is.StringContaining(errorMessage));            
            mtp.Verify(tp => tp.Decrypt(tokenString), Times.Once());
            mtvp.Verify(tvp => tvp.ValidateToken(token, It.IsAny<string>(), typeof(ValidateToken)), Times.Once());
        }
    }
}