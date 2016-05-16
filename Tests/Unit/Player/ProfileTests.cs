using System;
using System.Collections.Generic;
using System.Globalization;
using AFT.RegoV2.MemberApi.Interface.Player;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Player
{
    internal class ProfileTests : PlayerServiceTestsBase
    {
        private RegisterRequest _registrationData;

        public override void BeforeEach()
        {
            base.BeforeEach();

            _registrationData = RegisterPlayer().Result;
        }

        [Test]
        public async void Can_get_player_profile()
        {
            // Arrange
            await PlayerWebservice.Login(new LoginRequest
            {
                Username = _registrationData.Username,
                Password = _registrationData.Password,
                BrandId = new Guid("00000000-0000-0000-0000-000000000138"),
                IPAddress = "::1",
                RequestHeaders = new Dictionary<string, string>()
            });

            // Act
            var result = await PlayerWebservice.ProfileAsync();

            // Assert 
            Assert.AreEqual(_registrationData.IpAddress, result.IpAddress);
            Assert.AreEqual(_registrationData.DomainName, result.DomainName);
            Assert.AreEqual(_registrationData.Username, result.Username);
            Assert.AreEqual(_registrationData.FirstName, result.FirstName);
            Assert.AreEqual(_registrationData.LastName, result.LastName);
            Assert.AreEqual(_registrationData.MailingAddressLine1, result.MailingAddressLine1);
            Assert.AreEqual(_registrationData.MailingAddressPostalCode, result.MailingAddressPostalCode);
            Assert.AreEqual(_registrationData.CountryCode, result.CountryCode);
            Assert.AreEqual(_registrationData.CurrencyCode, result.CurrencyCode);
            Assert.AreEqual(_registrationData.Email, result.Email);
            Assert.AreEqual(_registrationData.PhoneNumber, result.PhoneNumber);
            Assert.AreEqual(_registrationData.DateOfBirth, result.DateOfBirth.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture));
        }
    }
}