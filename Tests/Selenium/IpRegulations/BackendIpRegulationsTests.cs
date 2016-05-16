using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    [Ignore("Until Maxim investigates")]
    class BackendIpRegulationsTests : SeleniumBaseForAdminWebsite
    {
        private BackendIpRegulationsPage _backendIpRegulationsPage;

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            var dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _backendIpRegulationsPage = dashboardPage.Menu.ClickBackendIpRegulationsMenuItem();
        }


        [Test]
        public void Can_create_backend_ip_regulation()
        {
            var form = _backendIpRegulationsPage.OpenNewBackendIpRegulationForm();
            var data = new BackendIpRegulationData
            {
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                AdvancedSettings = false,
                Description = TestDataGenerator.GetRandomStringWithSpecialSymbols(10)
            };
            var submittedBackendIpRegulationForm = form.Submit(data);

            Assert.AreEqual("IP Regulation has been successfully created", submittedBackendIpRegulationForm.ConfirmationMessage);
        }

        [Test]
        public void Can_edit_backend_ip_regulation()
        {
            var form = _backendIpRegulationsPage.OpenNewBackendIpRegulationForm();
            var data = new BackendIpRegulationData
            {
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                AdvancedSettings = false,
                Description = TestDataGenerator.GetRandomStringWithSpecialSymbols(10)
            };
            var submittedBackendIpRegulationForm = form.Submit(data);

            Assert.AreEqual("IP Regulation has been successfully created", submittedBackendIpRegulationForm.ConfirmationMessage);

            submittedBackendIpRegulationForm.Close();
            var editForm = _backendIpRegulationsPage.OpenEditBackendIpRegulationForm(data.IpAddress);
            var editData = new EditBackendIpRegulationData
            {
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                Description = TestDataGenerator.GetRandomStringWithSpecialSymbols(10)
            };
            
            editForm.ClearFieldsOnForm();
            var submittedBackendIpRegulationEditForm = editForm.Submit(editData);

            Assert.AreEqual("IP Regulation has been successfully updated", submittedBackendIpRegulationEditForm.ConfirmationMessage);
        }

        [Test]
        public void Can_delete_backend_ip_regulation()
        {
            var form = _backendIpRegulationsPage.OpenNewBackendIpRegulationForm();
            var data = new BackendIpRegulationData
            {
                IpAddress = TestDataGenerator.GetRandomIpAddress(),
                AdvancedSettings = false,
                Description = TestDataGenerator.GetRandomStringWithSpecialSymbols(10)
            };
            var submittedBackendIpRegulationForm = form.Submit(data);

            Assert.AreEqual("IP Regulation has been successfully created", submittedBackendIpRegulationForm.ConfirmationMessage);

            submittedBackendIpRegulationForm.Close();

            _backendIpRegulationsPage.DeleteIpRegulation(data.IpAddress);

            Assert.True(_backendIpRegulationsPage.IsIpRegulationExists(data.IpAddress));
        }

        [Test]
        public void Cannot_create_ip_regulation_with_invalid_ipv4()
        {
            var form = _backendIpRegulationsPage.OpenNewBackendIpRegulationForm();
            var data = new BackendIpRegulationData
            {
                IpAddress = TestDataGenerator.GetRandomIpAddress() + TestDataGenerator.GetRandomStringWithSpecialSymbols(5),
                AdvancedSettings = false,
                Description = TestDataGenerator.GetRandomStringWithSpecialSymbols(10)
            };
            var submittedBackendIpRegulationForm = form.Submit(data);

            Assert.AreEqual("IP address is invalid", submittedBackendIpRegulationForm.IpAddressValidation);
        }

        [Test]
        public void Cannot_create_ip_regulation_with_invalid_ipv6()
        {
            var form = _backendIpRegulationsPage.OpenNewBackendIpRegulationForm();
            var data = new BackendIpRegulationData
            {
                IpAddress = TestDataGenerator.GetRandomIpAddress(IpVersion.Ipv6) + TestDataGenerator.GetRandomString(10),
                AdvancedSettings = false,
                Description = TestDataGenerator.GetRandomStringWithSpecialSymbols(10)
            };
            var submittedBackendIpRegulationForm = form.Submit(data);

            Assert.AreEqual("IP address is invalid", submittedBackendIpRegulationForm.IpAddressValidation);
        }
    }
}
