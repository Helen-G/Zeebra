using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.Resources;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Validation
{
    class ToggleBonusStatusTests : BonusTestsBase
    {
        [Test]
        public void Can_not_change_status_of_non_existent_bonus()
        {
            var result = BonusQueries.GetValidationResult(new ToggleBonusStatusVM { Id = Guid.Empty, IsActive = true });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.BonusDoesNotExist);
        }

        [TestCase(Trigger.ReferFriend, Description = "Can not activate second refer friends bonus for brand")]
        [TestCase(Trigger.MobilePlusEmailVerification, Description = "Can not activate second verification bonus for brand")]
        public void Only_one_bonus_can_be_active(Trigger trigger)
        {
            var bonus1 = BonusHelper.CreateBasicBonus();
            bonus1.Template.Info.BonusTrigger = trigger;

            var bonus2 = BonusHelper.CreateBasicBonus(isActive: false);
            bonus2.Template.Info.BonusTrigger = trigger;
            bonus2.Template.Info.Brand = bonus1.Template.Info.Brand;

            var result = BonusQueries.GetValidationResult(new ToggleBonusStatusVM { Id = bonus2.Id, IsActive = true });

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.OneBonusOfATypeCanBeActive);
        }
    }
}