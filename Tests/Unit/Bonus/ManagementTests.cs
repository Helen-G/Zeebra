using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.EventHandlers;
using AFT.RegoV2.Core.Bonus.Resources;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Base;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus
{
    class ManagementTests : BonusTestsBase
    {
        [TestCase(true, Description = "Activating active bonus does nothing")]
        [TestCase(false, Description = "Deactivating inactive bonus does nothing")]
        public void Update_status_returns_if_input_status_matches_status_in_db(bool isActive)
        {
            var id = Guid.NewGuid();
            BonusRepository.Bonuses.Add(new Core.Bonus.Data.Bonus { Id = id, Template = new Template(), IsActive = isActive });

            BonusManagementCommands.ChangeBonusStatus(new ToggleBonusStatusVM { Id = id, IsActive = isActive });

            Assert.AreEqual(1, BonusRepository.Bonuses.Count(b => b.Id == id));
        }

        [Test]
        public void Can_not_edit_template_if_there_are_active_bonuses_using_it()
        {
            var template = BonusHelper.CreateFirstDepositTemplate();
            BonusRepository.Bonuses.Add(new Core.Bonus.Data.Bonus { Template = template, IsActive = true });

            var ex = Assert.Throws<RegoException>(() => BonusManagementCommands.AddUpdateTemplate(template));
            Assert.AreEqual(ValidatorMessages.AllBonusesShouldBeInactive, ex.Message);
        }

        [Test]
        public void Template_update_does_not_affect_already_redemed_bonuses()
        {
            var paymentSubscriber = Container.Resolve<PaymentSubscriber>();
            var bonus = BonusHelper.CreateBasicBonus(mode: IssuanceMode.AutomaticWithCode);
            bonus.Template.Rules.RewardType = BonusRewardType.Percentage;
            bonus.Template.Rules.RewardTiers.Single().Tiers.Single().MaxAmount = 10;
            BonusRepository.Templates.Add(bonus.Template);

            paymentSubscriber.Handle(new DepositSubmitted
            {
                Amount = 200,
                PlayerId = PlayerId,
                BonusCode = bonus.Code
            });

            var updatedBonus = BonusHelper.CreateBasicBonus();
            updatedBonus.Template.Id = bonus.Template.Id;
            updatedBonus.Template.Version = 1;
            updatedBonus.Version = 1;
            updatedBonus.Template.Rules.RewardTiers.Single().Tiers.Single().MaxAmount = 10;
            BonusRepository.Templates.Add(updatedBonus.Template);

            paymentSubscriber.Handle(new DepositApproved
            {
                ActualAmount = 200,
                PlayerId = PlayerId
            });
            Assert.AreEqual(10, BonusRedemptions.Single().Amount);
        }
    }
}