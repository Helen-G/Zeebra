using System;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Security
{
    class LoggingServiceTests : SecurityTestsBase
    {
        private LoggingService _service;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _service = Container.Resolve<LoggingService>();
        }

        [Test]
        public void Can_log_error()
        {
            /*** Arrange ***/
            var errorId = Guid.NewGuid();
            const string errorUser = "SuperAdmin";
            const string errorMessage = "Test error";
            const string errorType = "Exception";
            const string errorHostName = "AdminWebsite";
            const string errorSource = "Test";
            var errorDateTime = DateTime.Now;

            var testError = new Error
            {
                Id = errorId,
                Message = errorMessage,
                Type = errorType,
                HostName = errorHostName,
                Source = errorSource,
                Time = errorDateTime, 
                User = errorUser
            };

            /*** Act ***/
            _service.Log(testError);

            /*** Assert ***/
            var assertError = _service.GetError(errorId);

            Assert.IsNotNull(assertError);
            Assert.AreEqual(assertError.Message, errorMessage);
            Assert.AreEqual(assertError.Type, errorType);
            Assert.AreEqual(assertError.Source, errorSource);
            Assert.AreEqual(assertError.User, errorUser);
            Assert.AreEqual(assertError.HostName, errorHostName);
            Assert.AreEqual(assertError.Time, errorDateTime);
        }
    }
}
