using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Qualification
{
    class IssanceByCsQualificationTests : BonusTestsBase
    {
        [Test]
        public void ManualByCs_bonus_is_qualified_during_manual_issuance()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            BonusHelper.CreateBasicBonus(mode: IssuanceMode.ManualByCs);

            BonusQueries.GetManualByCsQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .NotBeEmpty();
        }

        [Test]
        public void Expired_bonus_is_qualifed_when_issued_by_Cs()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.ActiveFrom = bonus.ActiveFrom.AddDays(-1);
            bonus.ActiveTo = bonus.ActiveTo.AddDays(-1);

            BonusQueries.GetManualByCsQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .NotBeEmpty();
        }

        [Test]
        public void Bonus_that_become_active_in_future_is_not_qualified_when_issued_by_Cs()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.ActiveFrom = bonus.ActiveFrom.AddDays(2);
            bonus.ActiveTo = bonus.ActiveTo.AddDays(2);

            BonusQueries.GetManualByCsQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .BeEmpty();
        }

        [Test]
        public void Bonus_issued_by_Cs_ignores_bonus_application_duration_qualification()
        {
            PaymentHelper.MakeDeposit(PlayerId);
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.DurationType = DurationType.Custom;
            bonus.DurationStart = SystemTime.Now.AddMinutes(5);
            bonus.DurationEnd = SystemTime.Now.AddMinutes(10);

            SystemTime.Factory = () => DateTimeOffset.Now.AddMinutes(11);

            BonusQueries.GetManualByCsQualifiedBonuses(PlayerId)
                .ToList()
                .Should()
                .NotBeEmpty();

            SystemTime.Factory = () => DateTimeOffset.Now;
        }
    }
}