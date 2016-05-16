using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.Tests.Common;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration
{
    internal class RegistrationTests : PlayerServiceTestsBase
    {
        [Test]
        public void Can_register_Player()
        {
            var registrationData = TestDataGenerator.CreateRandomRegistrationRequestData();
            var response = ServiceProxy.Register(registrationData);

            Assert.That(response.UserId, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void Register_Fail_If_Invalid_Request_Post()
        {
            var registrationData = TestDataGenerator.CreateRandomRegistrationRequestData();
            registrationData.Username = null;
            bool exceptionThrown = false;
            try
            {
                ServiceProxy.Register(registrationData);
            }
            catch (MemberApiProxyException ex)
            {
                var e = ex.Exception;
                exceptionThrown = true;
                var responseErrors = e.Violations;
                Assert.That(responseErrors.Count,Is.GreaterThanOrEqualTo(1));
                var userNameError = responseErrors.FirstOrDefault(err => err.FieldName == "Username");
                Assert.IsNotNull(userNameError);
            }
            Assert.True(exceptionThrown);
        }
    }
}