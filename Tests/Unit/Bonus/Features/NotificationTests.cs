using System;
using System.Linq;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Features
{
    class NotificationTests : BonusTestsBase
    {
        [Test]
        public void Email_is_sent_upon_bonus_activation()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Notification.EmailTemplateId = Guid.NewGuid();
            PaymentHelper.MakeDeposit(PlayerId);

            ServiceBus.PublishedCommandCount.Should().Be(1);
        }

        [Test]
        public void Sms_is_sent_upon_bonus_activation()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Notification.SmsTemplateId = Guid.NewGuid();
            PaymentHelper.MakeDeposit(PlayerId);

            ServiceBus.PublishedCommandCount.Should().Be(1);
        }

        [Test]
        public void Email_is_not_sent_if_address_is_unknown_upon_bonus_activation()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Notification.EmailTemplateId = Guid.NewGuid();
            BonusRepository.Players.Single(p => p.Id == PlayerId).Email = string.Empty;
            PaymentHelper.MakeDeposit(PlayerId);

            ServiceBus.PublishedCommandCount.Should().Be(0);
        }

        [Test]
        public void Sms_is_not_sent_if_address_is_unknown_upon_bonus_activation()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            bonus.Template.Notification.SmsTemplateId = Guid.NewGuid();
            BonusRepository.Players.Single(p => p.Id == PlayerId).PhoneNumber = string.Empty;
            PaymentHelper.MakeDeposit(PlayerId);

            ServiceBus.PublishedCommandCount.Should().Be(0);
        }
    }
}