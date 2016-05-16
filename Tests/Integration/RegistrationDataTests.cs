using System;
using AFT.RegoV2.MemberApi.Interface.Account;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Integration
{
    internal class RegistrationDataTests : PlayerServiceTestsBase
    {
        [Test]
        public void Can_get_registration_data()
        {
            ServiceProxy.RegistrationFormData(new RegistrationFormDataRequest
            {
                BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                CultureCode = "zh-TW"
            });

            
        }
    }
}