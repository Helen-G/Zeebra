using System;
using System.Linq;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Commands;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Domain.Player.Data;
using Microsoft.Practices.Unity;
using Moq;
using NUnit.Framework;
using ServiceStack.Validation;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class SendNewPasswordTests : PlayerServiceTestsBase
    {
        private PlayerCommands _playerCommands;
        public override void BeforeEach()
        {
            base.BeforeEach();
            var result = RegisterPlayer().Result;
        }

        [TestCase("short")]
        [TestCase("too_long_to_be_real_password")]
        public void Password_length_validation_works(string password)
        {
            _playerCommands = Container.Resolve<PlayerCommands>();
            var p = FakePlayerRepository.Players.FirstOrDefault();
            Assert.That(p, Is.Not.Null);
            var e = Assert.Throws<ValidationError>(
                () => _playerCommands.SendNewPassword(
                    new SendNewPasswordData
                    {
                        NewPassword = password, 
                        PlayerId = p.Id, 
                        SendBy = SendBy.Email
                    }));

            Assert.IsNotEmpty(e.ErrorMessage);
            Assert.AreEqual(1, e.Violations.Count);
        }

        [Test]
        public void Invalid_PlayerId_Verification_Works()
        {
            _playerCommands = Container.Resolve<PlayerCommands>();
            var e = Assert.Throws<ValidationError>(
                () => _playerCommands.SendNewPassword(
                    new SendNewPasswordData
                    {
                        NewPassword = "correct", 
                        PlayerId = Guid.NewGuid(), 
                        SendBy = SendBy.Email
                    }));

            Assert.IsNotEmpty(e.ErrorMessage);
            Assert.AreEqual(1, e.Violations.Count);
        }

        [TestCase(SendBy.Email)]
        [TestCase(SendBy.Sms)]
        public void Correct_Call_NotificationService(SendBy sendBy)
        {
            var busMock = new Mock<IServiceBus>();
            Container.RegisterInstance<IServiceBus>(busMock.Object);

            _playerCommands = Container.Resolve<PlayerCommands>();

            var player = FakePlayerRepository.Players.FirstOrDefault();
            Assert.That(player, Is.Not.Null);

            const string newPassword = "correct";
            _playerCommands.SendNewPassword(new SendNewPasswordData { NewPassword = newPassword, PlayerId = player.Id, SendBy = sendBy });

            if (sendBy == SendBy.Email)
            {
                busMock.Verify(
                    n => n.PublishMessage(
                        It.IsAny<EmailCommandMessage>()),
                    Times.Once);
                busMock.Verify(
                    n => n.PublishMessage(
                        It.IsAny<SmsCommandMessage>()),
                    Times.Never);
            }
            else
            {
                busMock.Verify(
                    n => n.PublishMessage(
                        It.IsAny<SmsCommandMessage>()),
                    Times.Once);
                busMock.Verify(n => n.PublishMessage(
                    It.IsAny<EmailCommandMessage>()),
                    Times.Never);
            }
        }
    }
}