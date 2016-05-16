using System;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Infrastructure.DataAccess.Payment.Repository;
using AFT.RegoV2.Tests.Common.Base;
using AFT.RegoV2.Tests.Common.Extensions;
using AFT.RegoV2.Tests.Common.Helpers;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Selenium
{
    class TransferSettingsFundInFundOutTests : SeleniumBaseForMemberWebsite
    {
        private RegistrationDataForMemberWebsite _player;
        private PlayerProfilePage _playerProfilePage;
        protected readonly Guid DefaultBrandId = new Guid("00000000-0000-0000-0000-000000000138");
        private Core.Brand.Data.Brand _brand;
        private const decimal MinAmountPerTransaction = 5;
        private const decimal MaxAmountPerTransaction = 100;

        public override void BeforeAll()
        {
            base.BeforeAll();
            var brandTestHelper = _container.Resolve<BrandTestHelper>();
            var transferTestHelper = _container.Resolve<TransferFundTestHelper>();
            var paymentTestHelper = _container.Resolve<PaymentTestHelper>();
            var playerTestHelper = _container.Resolve<PlayerTestHelper>();
            var playerQueries = _container.Resolve<PlayerQueries>();
            var playerCommands = _container.Resolve<PlayerCommands>();
            var brandQueries = _container.Resolve<BrandQueries>();

            _brand = brandQueries.GetBrand(DefaultBrandId);
            
            var vipLevel = _brand.DefaultVipLevel;
            
            //create fund-in transfer settings for the brand and vip level
            var transferSettings = new SaveTransferSettingsCommand
            {
                MinAmountPerTransaction = MinAmountPerTransaction,
                MaxAmountPerTransaction = MaxAmountPerTransaction
            };
            transferTestHelper.CreateTransferSettings(_brand, transferSettings, TransferFundType.FundIn, "CAD", vipLevel.Id);
           
            //create a player
           _player = playerTestHelper.CreatePlayerForMemberWebsite(currencyCode:"CAD", password:"123456");

            //deposit money to the player's main balance
            paymentTestHelper.MakeDeposit(_player.Username, 200);
            var playerId = playerQueries.GetPlayerByUsername(_player.Username).Id;

            //change the vip level of the player
            playerCommands.ChangeVipLevel(playerId, vipLevel.Id, "changed vip level");
        }

        public override void BeforeEach()
        {
            base.BeforeEach();
            _playerProfilePage = _driver.LoginToMemberWebsite(_player.Username, _player.Password);
        }

        [Test]
        public void Cannot_fund_in_amount_outside_transfer_settings_boundaries()
        {
            var balanceInfoPage = _playerProfilePage.Menu.ClickBalanceInformationMenu();
            var transferFundPage = balanceInfoPage.Menu.ClickTransferFundSubMenu();

            const decimal minAmountPerTransaction = MinAmountPerTransaction - 1;
            const decimal maxAmountPerTransaction = MaxAmountPerTransaction + 1;
            
            transferFundPage.TryToMakeInvalidTransferFundRequest(TransferFundType.FundIn, "Product 138", minAmountPerTransaction.ToString());
            Assert.That(transferFundPage.ValidationMessage, Is.StringContaining("Transfer failed. The entered amount is below the allowed value."));

            transferFundPage.TryToMakeInvalidTransferFundRequest(TransferFundType.FundIn, "Product 138", maxAmountPerTransaction.ToString());
            Assert.That(transferFundPage.ValidationMessage, Is.StringContaining("Transfer failed. The entered amount exceeds the allowed value."));
        }
        
    }
}