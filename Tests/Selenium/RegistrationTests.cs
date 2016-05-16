using System.Linq;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    internal class PlayerRegistrationTests : SeleniumBaseForMemberWebsite
    {
        private RegisterPage _registerPage;
        private RegistrationDataForMemberWebsite _data;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _registerPage = new RegisterPage(_driver);
            _registerPage.NavigateToMemberWebsite();
            _data = TestDataGenerator.CreateValidPlayerDataForMemberWebsite("CAD", "en-US", "CA");
        }

        public void CanOpenRegisterPage()
        {
            Assert.That(_registerPage.Url.ToString(), Is.StringContaining("Register"));
        }

        [Test]
        public void Can_register_on_member_website()
        {
            var registrationSuccess = _registerPage.Register(_data);
            Assert.AreEqual(true, registrationSuccess);
            var playerProfilePage = _registerPage.GoToPlayerProfile();
            Assert.That(playerProfilePage.Url.ToString(), Is.StringContaining("PlayerProfile"));
        }

        [Test]
        public void Cannot_register_with_duplicate_data()
        {
            _registerPage.Register(_data);
            var playerProfilePage = _registerPage.GoToPlayerProfile();
            playerProfilePage.Logout();

            _registerPage = new RegisterPage(_driver);
            _driver.WaitForJavaScript();
            _registerPage.NavigateToMemberWebsite();
            var registrationSuccess = _registerPage.Register(_data);
            var messages = _registerPage.GetErrorMessages().ToArray();

            Assert.AreEqual(false, registrationSuccess);
            Assert.That(messages.Any(m => m.Contains("Username already exists")));
            Assert.That(messages.Any(m => m.Contains("email already exists")));
        }

        [Test]
        public void Cannot_register_using_only_spaces()
        {
            var data = TestDataGenerator.CreateRegistrationDataWithSpacesOnly();
            _registerPage.RegisterWithInvalidData(data);
            var messages = _registerPage.GetErrorMessages();

            Assert.That(messages.Any(a => a.Contains("is required")));
        }

        [Test]
        public void Cannot_register_with_data_exceeding_max_limit()
        {
           var data = TestDataGenerator.RegistrationDataExceedsMaxLimit();
            _registerPage.RegisterWithInvalidData(data);
            var messages = _registerPage.GetErrorMessages();

            Assert.That(messages.Any(m => m.Contains("length is not in its allowed range")));
        }
    }
}
