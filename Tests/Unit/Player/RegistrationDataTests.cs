using System;
using System.Linq;
using System.Threading.Tasks;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.Tests.Common;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class RegistrationDataTests : PlayerServiceTestsBase
    {
        private RegistrationFormDataResponse _response;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _response = Task.Run(() => PlayerWebservice.RegistrationFormDataAsync(new RegistrationFormDataRequest
                {
                    BrandId = new Guid("00000000-0000-0000-0000-000000000138")
                })).Result;
        }

        [Test]
        public void Can_get_currency_codes()
        {
            Assert.AreEqual(TestDataGenerator.CurrencyCodes.Length, _response.CurrencyCodes.Length);
            Assert.That(_response.CurrencyCodes.All(a => a != string.Empty));
        }

        [Test]
        public void Can_get_country_codes()
        {
            Assert.AreEqual(TestDataGenerator.CountryCodes.Length, _response.CountryCodes.Length);
            Assert.That(_response.CountryCodes.All(a => a != string.Empty));
        }
    }
}
