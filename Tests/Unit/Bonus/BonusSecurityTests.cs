using System;
using System.Collections.ObjectModel;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus
{
    class BonusSecurityTests : BonusTestsBase
    {
        private User _currentUser;

        public override void BeforeEach()
        {
            base.BeforeEach();

            var repo = Container.Resolve<ISecurityRepository>();
            _currentUser = repo.Users.Single();
            _currentUser.Role.Id = Guid.NewGuid();
        }

        [Test]
        public void Cannot_access_bonuses_that_are_not_allowed_to_user()
        {
            var bonus1 = BonusHelper.CreateBasicBonus();
            var bonus2 = BonusHelper.CreateBasicBonus();

            var forbiddenBrand = BrandHelper.CreateBrand(isActive: true);
            bonus2.Template.Info.Brand = BonusRepository.Brands.Single(b => b.Id == forbiddenBrand.Id);

            _currentUser.AllowedBrands = new Collection<BrandId> { new BrandId { Id = bonus1.Template.Info.Brand.Id } };

            var bonuses = BonusQueries.GetCurrentVersionBonuses();

            bonuses.Count().Should().Be(1);
            bonuses.Single().Id.Should().Be(bonus1.Id);
        }

        [Test]
        public void Cannot_access_templates_that_are_not_allowed_to_user()
        {
            var template1 = BonusHelper.CreateFirstDepositTemplate();
            var template2 = BonusHelper.CreateFirstDepositTemplate();

            var forbiddenBrand = BrandHelper.CreateBrand(isActive: true);
            template2.Info.Brand = BonusRepository.Brands.Single(b => b.Id == forbiddenBrand.Id);

            _currentUser.AllowedBrands = new Collection<BrandId> { new BrandId { Id = template1.Info.Brand.Id } };

            var templates = BonusQueries.GetCurrentVersionTemplates();

            templates.Count().Should().Be(1);
            templates.Single().Id.Should().Be(template1.Id);
        }

        [Test]
        public void Cannot_access_bonus_redemptions_that_are_not_allowed_to_user()
        {
            var bonus1 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            var bonus2 = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus2.Template.Info.DepositKind = DepositKind.Reload;

            PaymentHelper.MakeDeposit(PlayerId, bonusCode: bonus1.Code);
            PaymentHelper.MakeDeposit(PlayerId, bonusCode: bonus2.Code);

            var forbiddenBrand = BrandHelper.CreateBrand(isActive: true);
            bonus2.Template.Info.Brand = BonusRepository.Brands.Single(b => b.Id == forbiddenBrand.Id);

            _currentUser.AllowedBrands = new Collection<BrandId> { new BrandId { Id = bonus1.Template.Info.Brand.Id } };

            var bonusRedemptions = BonusQueries.GetBonusRedemptions();

            bonusRedemptions.Count().Should().Be(1);
            bonusRedemptions.Single().Bonus.Id.Should().Be(bonus1.Id);
        }
    }
}
