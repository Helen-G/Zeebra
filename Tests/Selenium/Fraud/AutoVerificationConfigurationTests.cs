using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Pages.BackEnd;
using AFT.RegoV2.Tests.Common.Pages.BackEnd.Fraud;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium.Fraud
{
    /// <summary>
    /// Represents tests related to Fraud -> Auto Verification Configuration
    /// </summary>
    class AutoVerificationConfigurationTests : SeleniumBaseForAdminWebsite
    {
        private AutoVerificationConfigurationPage _autoVerificationConfigurationPage;
        private NewAutoVerificationConfigurationForm _newAvcForm;
        private DashboardPage _dashboardPage;
        private VipLevelData _vipLevelData;
        private AutoVerificationConfigurationData avcData;
        private const string DefaultLicensee = "Flycow";
        private const string DefaultBrand = "138";
        private const string DefaultCurrency = "CAD";

        public override void BeforeAll()
        {
            base.BeforeAll();

            //create vip level for a brand
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            var vipLevelManagerPage = _dashboardPage.Menu.ClickVipLevelManagerMenuItem();
            var newVipLevelPage = vipLevelManagerPage.OpenNewVipLevelForm();
            _vipLevelData = TestDataGenerator.CreateValidVipLevelData(DefaultLicensee, DefaultBrand, false);
            var submittedVipLevelForm = newVipLevelPage.Submit(_vipLevelData);
            submittedVipLevelForm.CloseTab("View VIP Level");

            //generate auto verification configuration form data
            avcData = TestDataGenerator.CreateAutoVerificationConfigurationData(
                 DefaultLicensee,
                 DefaultBrand,
                 DefaultCurrency,
                 _vipLevelData.Name
               );
            
             _autoVerificationConfigurationPage = submittedVipLevelForm.Menu.ClickAutoVerificationConfiguration();
             _newAvcForm = _autoVerificationConfigurationPage.OpenNewAutoVerificationForm();
             _newAvcForm.SubmitAutoVerificationConfiguration(avcData).CloseTab("View Auto Verification Configuration");
            
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _driver.Logout();
            _dashboardPage = _driver.LoginToAdminWebsiteAsSuperAdmin();
            _autoVerificationConfigurationPage = _dashboardPage.Menu.ClickAutoVerificationConfiguration();
        }
       
     
        [Test]
        public void Can_edit_auto_verification_configuration_via_UI()
        {
            //generate auto verification configuration edited form data
            var avcDataEdited = TestDataGenerator.CreateAutoVerificationConfigurationData(
                 DefaultLicensee,
                 DefaultBrand,
                 "CNY",
                 _vipLevelData.Name
               );

            //edit avc
            var editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(avcData);
            var viewAvcForm = editAvcForm.Submit(avcDataEdited);


            //TODO the message text should be changed: bug AFTREGO-3068 
            Assert.AreEqual("Auto Verification Configurations has been successfuly created.", viewAvcForm.SuccessAlert.Text);
            Assert.AreEqual(avcDataEdited.Licensee, viewAvcForm.Licensee.Text);
            Assert.AreEqual(avcDataEdited.Brand, viewAvcForm.Brand.Text);
            Assert.AreEqual(avcDataEdited.Currency, viewAvcForm.Currency.Text);
            Assert.AreEqual(avcDataEdited.VipLevel, viewAvcForm.VipLevel.Text);

            viewAvcForm.CloseTab("View Auto Verification Configuration");

            //move to the initial state
            editAvcForm = _autoVerificationConfigurationPage.OpenEditAutoVerificationConfigurationForm(avcDataEdited);
            viewAvcForm = editAvcForm.Submit(avcData);
            //TODO the message text should be changed: bug AFTREGO-3068 
            Assert.AreEqual("Auto Verification Configurations has been successfuly created.", viewAvcForm.SuccessAlert.Text);
        }

        //TODO: Svitlana
        [Test, Ignore]
        public void Can_activate_deactive_auto_verification_configuration()
        {

        }

        [Test]
        public void Can_not_create_duplicate_auto_verification_configurationvia_via_UI()
        {
            _newAvcForm = _autoVerificationConfigurationPage.OpenNewAutoVerificationForm();
            _newAvcForm.SubmitAutoVerificationConfiguration(avcData);
           
            var failAvc = new AutoVerificationConfigurationFailure(_driver);

            Assert.True(failAvc.ErrorAlert.Displayed);
            Assert.AreEqual(failAvc.ErrorAlert.Text, "You have already set up Auto Verification Check with the selected Brand, " +
                                                     "Currency and Vip level. Please, update the existing one or change your form data.");
            failAvc.CloseTab("Auto Verification Configuration Failure");
        }
        
    }
}
