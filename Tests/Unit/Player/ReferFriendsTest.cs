using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Domain.Player.Resources;
using AFT.RegoV2.MemberApi.Interface;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.MemberApi.Interface.Proxy;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class ReferFriendsTest : PlayerServiceTestsBase
    {
        private RegisterRequest _registrationData;
        public override void BeforeEach()
        {
            base.BeforeEach();

            _registrationData = RegisterPlayer().Result;
            
        }

        [Test]
        public async void Can_refer_friends()
        {
            await PlayerWebservice.Login(CreateLoginRequest (_registrationData.Username, _registrationData.Password ));
            var response = PlayerWebservice.ReferFriendsAsync(new ReferFriendsRequest { PhoneNumbers = new List<string> { "12345678" } });
        }

        [Test]
        public async void No_phone_numbers_validation_works()
        {
            await PlayerWebservice.Login(CreateLoginRequest (_registrationData.Username, _registrationData.Password ));
            var e = Assert.Throws<MemberApiProxyException>(async () => await PlayerWebservice.ReferFriendsAsync(new ReferFriendsRequest())).Exception;

            Assert.IsNotEmpty(e.ErrorMessage);
            Assert.AreEqual(1, e.Violations.Count());
            Assert.AreEqual(ReferalDataValidatorResponseCodes.PhoneNumbersAreMissing.ToString(), e.Violations.Single().ErrorMessage);
        }

        [TestCase(null)]
        [TestCase("        ")]
        [TestCase("1234567")]
        [TestCase("12345678910123456")]
        public async void Phone_numbers_validation_works(string phoneNumber)
        {
            await PlayerWebservice.Login(CreateLoginRequest (_registrationData.Username, _registrationData.Password ));
            var e = Assert.Throws<MemberApiProxyException>(async () => await PlayerWebservice.ReferFriendsAsync(new ReferFriendsRequest { PhoneNumbers = new List<string> { phoneNumber } })).Exception;

            Assert.IsNotEmpty(e.ErrorMessage);
            Assert.AreEqual(1, e.Violations.Count());
            Assert.AreEqual(ReferalDataValidatorResponseCodes.PhoneNumbersAreNotValid.ToString(), e.Violations.Single().ErrorMessage);
        }

        private LoginRequest CreateLoginRequest(string username, string password)
        {
            return new LoginRequest
            {
                Username = username,
                Password = password,
                BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                IPAddress = "::1",
                RequestHeaders = new Dictionary<string, string>()
            };
        }

    }
}
