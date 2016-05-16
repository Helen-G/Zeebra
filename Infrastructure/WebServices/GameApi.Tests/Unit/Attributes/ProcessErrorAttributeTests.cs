using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using AFT.RegoV2.Core.Game.Services;
using AFT.RegoV2.GameApi.Interface.Attributes;
using AFT.RegoV2.GameApi.Interface.Classes;
using AFT.RegoV2.GameApi.Interface.Services;
using AFT.RegoV2.Infrastructure.Providers;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AFT.RegoV2.GameApi.Tests.Unit.Attributes
{
    [TestFixture]
    [Category("Unit")]
    public class ProcessErrorAttributeTests 
    {
        [Test]
        public void If_no_exception_no_response_is_set()
        {
            // 1. Arrange
            var attr = new ProcessErrorAttribute();
            var context = new HttpActionExecutedContext {Exception = null};
            // 2.Act
            attr.OnActionExecuted(context);
            // 3. Assert
            context.Response.Should().BeNull();
        }

        [Test]
        public void On_exception_response_is_set()
        {
            // 1. Arrange
            var attr = new ProcessErrorAttribute();
            var errorMessage = Guid.NewGuid().ToString();
            var exception = new Exception(errorMessage);
            var context = new HttpActionExecutedContext
            {
                Exception = exception,
                ActionContext = new HttpActionContext()
            };

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
            attr.OnActionExecuted(context);

            // 3. Assert
            Assert.That(context.Response.Content.ReadAsStringAsync().Result, Is.StringContaining(errorMessage));
            mLog.Verify(l => l.LogError(It.IsAny<string>(), exception), Times.Once());
        }
    }
}