using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Tests.Common;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Qualification
{
    class PreRedemptionQualificationTests : BonusTestsBase
    {
        [Test]
        public void Can_get_qualified_bonuses_list()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);

            BonusQueries.GetOfflineDepositQualifiedBonuses(PlayerId)
                .ToList()
                .Single()
                .Name
                .Should()
                .Be(bonus.Name);
        }

        [Test]
        public void Qualified_bonuses_list_does_not_contain_expired_bonuses()
        {
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.ActiveFrom = DateTimeOffset.MaxValue;

            BonusQueries.GetOfflineDepositQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .BeEmpty();
        }

        [Test]
        public void Qualified_bonuses_list_does_not_contain_inactive_bonuses()
        {
            BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode, isActive: false);

            BonusQueries.GetOfflineDepositQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .BeEmpty();
        }

        [Test]
        public void Qualified_bonuses_list_contain_current_bonus_version()
        {
            var template = BonusHelper.CreateFirstDepositTemplate(mode: IssuanceMode.AutomaticWithCode);
            var bonusV0 = new Core.Bonus.Data.Bonus
            {
                Id = Guid.Empty,
                Template = template,
                Name = TestDataGenerator.GetRandomString(),
                Code = TestDataGenerator.GetRandomString(),
                IsActive = true,
                ActiveTo = DateTimeOffset.Now.Date.AddDays(1)
            };
            BonusManagementCommands.AddBonus(bonusV0);

            var bonusV1 = new Core.Bonus.Data.Bonus
            {
                Id = bonusV0.Id,
                Template = template,
                Name = TestDataGenerator.GetRandomString(),
                Code = TestDataGenerator.GetRandomString(),
                IsActive = true,
                ActiveTo = DateTimeOffset.Now.Date.AddDays(1)
            };
            BonusManagementCommands.UpdateBonus(bonusV1);

            BonusQueries
                .GetOfflineDepositQualifiedBonuses(PlayerId)
                .Single()
                .Code
                .Should()
                .Be(bonusV1.Code);
        }
    }
}