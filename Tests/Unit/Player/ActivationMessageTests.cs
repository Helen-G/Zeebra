using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Player.Events;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using WinService.Workers;
using WinService.Workers.Player;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class ActivationMessageTests : PlayerServiceTestsBase
    {
        private PlayerCommands _playerCommands;
        private EmailNotificationWorker _emailNotificationWorker;
        private PlayerActivationSmsNotificationWorker _activationSmsNotificationWorker;

        public override void BeforeEach()
        {
            base.BeforeEach();
            
            _playerCommands = Container.Resolve<PlayerCommands>();
            _emailNotificationWorker = Container.Resolve<EmailNotificationWorker>();
            _activationSmsNotificationWorker = Container.Resolve<PlayerActivationSmsNotificationWorker>();
        }

        [Test]
        public async void Can_send_activation_email()
        {
            _emailNotificationWorker.Start();

            FakeBrandRepository.Brands.First().PlayerActivationMethod = PlayerActivationMethod.Email;
            FakeBrandRepository.SaveChanges();

            await RegisterPlayer(false);
            var events = FakeEventRepository.GetEvents<NotificationSentEvent>().ToArray();

            Assert.That(events.Length, Is.EqualTo(1));
            Assert.That(events.First().Type, Is.EqualTo(NotificationType.Email));
        }

        [Test]
        public async void Can_send_activation_sms()
        {
            _activationSmsNotificationWorker.Start();

            FakeBrandRepository.Brands.First().PlayerActivationMethod = PlayerActivationMethod.Sms;
            FakeBrandRepository.SaveChanges();

            await RegisterPlayer(false);
            var events = FakeEventRepository.GetEvents<ActivationLinkSent>().ToArray();

            Assert.That(events.Length, Is.EqualTo(1));
            Assert.That(events.First().ContactType, Is.EqualTo(ContactType.Mobile));
        }

        [Test]
        public async void Can_send_activation_email_and_sms()
        {
            _emailNotificationWorker.Start();
            _activationSmsNotificationWorker.Start();

            FakeBrandRepository.Brands.First().PlayerActivationMethod = PlayerActivationMethod.EmailOrSms;
            FakeBrandRepository.SaveChanges();

            await RegisterPlayer(false);
            var notificationSentEvents = FakeEventRepository.GetEvents<NotificationSentEvent>().ToArray();

            Assert.That(notificationSentEvents.Length, Is.EqualTo(2));
            Assert.That(notificationSentEvents.Count(x => x.Type == NotificationType.Email), Is.EqualTo(1));
            Assert.That(notificationSentEvents.Count(x => x.Type == NotificationType.Sms), Is.EqualTo(1));
        }

        [Test]
        public async void Can_resend_activation_email()
        {
            _emailNotificationWorker.Start();

            FakeBrandRepository.Brands.First().PlayerActivationMethod = PlayerActivationMethod.Email;
            FakeBrandRepository.SaveChanges();

            await RegisterPlayer(false);
            var playerId = FakePlayerRepository.Players.First().Id;
            _playerCommands.ResendActivationEmail(playerId);

            var events = FakeEventRepository.GetEvents<NotificationSentEvent>().ToArray();

            Assert.That(events.Length, Is.EqualTo(2));
        }

        [Test]
        public async void Can_send_mobile_verification_sms()
        {
            await RegisterPlayer(false);
            var playerId = FakePlayerRepository.Players.First().Id;
            _playerCommands.SendMobileVerificationCode(playerId);

            var events = FakeEventRepository.GetEvents<MobileVerificationCodeSentSms>().ToArray();

            Assert.That(events.Length, Is.EqualTo(1));
            Assert.That(events.First().PlayerId, Is.EqualTo(playerId));
        }
    }
}