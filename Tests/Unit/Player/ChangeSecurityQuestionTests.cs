using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Domain.Player.Resources;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class ChangeSecurityQuestionTests: PlayerServiceTestsBase
    {
        private RegisterRequest _registrationData;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _registrationData = RegisterPlayer().Result;
        }

        [Test]
        public async void Can_change_security_question()
        {
            var player = FakePlayerRepository.Players.Single();
            var newQuestionId =TestDataGenerator.GetRandomSecurityQuestion();
            var newAnswer = "SecurityAnswer" + TestDataGenerator.GetRandomString();
            await PlayerWebservice.ChangeSecurityQuestionAsync(new ChangeSecurityQuestionRequest 
            { 
               Id = player.Id.ToString(),
               SecurityQuestionId = newQuestionId,
               SecurityAnswer = newAnswer,
            });

            //Assert.False(result.IsErrorResponse());
            Assert.AreEqual(newQuestionId, player.SecurityQuestionId.ToString());
            Assert.AreEqual(newAnswer, player.SecurityAnswer);
        }

        [Test]
        public void Cannt_change_security_question_with_invalid_question_id()
        {
            var player = FakePlayerRepository.Players.Single();
            var newQuestionId = (new Guid()).ToString();
            var newAnswer = "SecurityAnswer" + TestDataGenerator.GetRandomString();
            var e = Assert.Throws<MemberApiProxyException>(async () => await PlayerWebservice.ChangeSecurityQuestionAsync(new ChangeSecurityQuestionRequest
            {
                Id = player.Id.ToString(),
                SecurityQuestionId = newQuestionId,
                SecurityAnswer = newAnswer,
            })).Exception;
            Assert.IsNotEmpty(e.ErrorMessage);
            Assert.AreEqual(1, e.Violations.Count);
            Assert.AreEqual(RegisterValidatorResponseCodes.InvalidSecurityQuestionId.ToString(), e.Violations.Single().ErrorMessage);        
        }

        [Test]
        public void Cannt_change_security_question_with_blank_answer()
        {
            var player = FakePlayerRepository.Players.Single();
            var newQuestionId = TestDataGenerator.GetRandomSecurityQuestion();
            var newAnswer = "";
            var e = Assert.Throws<MemberApiProxyException>(async () => await PlayerWebservice.ChangeSecurityQuestionAsync(new ChangeSecurityQuestionRequest
            {
                Id = player.Id.ToString(),
                SecurityQuestionId = newQuestionId,
                SecurityAnswer = newAnswer,
            })).Exception;

            Assert.IsNotEmpty(e.ErrorMessage);
            Assert.AreEqual(1, e.Violations.Count);
            Assert.AreEqual(RegisterValidatorResponseCodes.SecurityAnswerIsMissing.ToString(), e.Violations.Single().ErrorMessage);
        }
        [Test]
        public void Cannt_change_security_question_with_invalid_player()
        {
            var playerId = (new Guid()).ToString();
            var newQuestionId = TestDataGenerator.GetRandomSecurityQuestion();
            var newAnswer = "SecurityAnswer" + TestDataGenerator.GetRandomString();
            var e = Assert.Throws<MemberApiProxyException>(async () => await PlayerWebservice.ChangeSecurityQuestionAsync(new ChangeSecurityQuestionRequest
            {
                Id = playerId, 
                SecurityQuestionId = newQuestionId,
                SecurityAnswer = newAnswer,
            })).Exception;

            Assert.IsNotEmpty(e.ErrorMessage);
            Assert.AreEqual(1, e.Violations.Count);
            Assert.AreEqual(PlayerAccountResponseCode.PlayerDoesNotExist.ToString(), e.Violations.Single().ErrorMessage);
        }
    }
}
