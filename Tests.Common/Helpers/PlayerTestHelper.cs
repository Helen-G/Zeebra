using System;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Bonus;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using AutoMapper;
using Player = AFT.RegoV2.Core.Player.Data.Player;

namespace AFT.RegoV2.Tests.Common.Helpers
{
    public class PlayerTestHelper
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IBonusRepository _bonusRepository;
        private readonly PlayerCommands _playerCommands;
        private readonly PlayerQueries _playerQueries;
        private readonly BrandQueries _brandQueries;

        public PlayerTestHelper(
            IBrandRepository brandRepository,
            IBonusRepository bonusRepository,
            PlayerCommands playerCommands,
            PlayerQueries playerQueries,
            BrandQueries brandQueries)
        {
            _brandRepository = brandRepository;
            _bonusRepository = bonusRepository;
            _playerCommands = playerCommands;
            _playerQueries = playerQueries;
            _brandQueries = brandQueries;
        }

        public Guid CreatePlayer(Guid? referredBy, bool isActive = true, Guid? brandId = null)
        {
            var playerRegData = TestDataGenerator.CreateRandomRegistrationRequestData();
            var registrationData = Mapper.DynamicMap<RegistrationData>(playerRegData);

            brandId = brandId ?? _brandRepository.Brands.First().Id;
            var brand = _brandQueries.GetBrandOrNull(brandId.Value);
            registrationData.BrandId = brand.Id.ToString();
            registrationData.CountryCode = brand.BrandCountries.First().Country.Code;
            registrationData.CurrencyCode = brand.BrandCurrencies.First().CurrencyCode;
            registrationData.CultureCode = brand.BrandCultures.First().CultureCode;
            registrationData.AccountStatus = isActive
                ? AccountStatus.Active.ToString()
                : AccountStatus.Inactive.ToString();
            registrationData.AccountAlertEmail = true;
            registrationData.AccountAlertSms = true;

            if (referredBy != null)
            {
                var referrer = _bonusRepository.Players.Single(a => a.Id == referredBy);
                registrationData.ReferralId = referrer.ReferralId.ToString();
            }

            return _playerCommands.Register(registrationData);
        }

        public Player CreatePlayer(bool isActive = true, Guid? brandId = null)
        {
            var playerId = CreatePlayer(null, isActive, brandId);

            return _playerQueries.GetPlayer(playerId);
        }

        public RegistrationDataForMemberWebsite CreatePlayerForMemberWebsite(string currencyCode = null, string password = null)
        {
            var playerRegData = TestDataGenerator.CreateValidPlayerDataForMemberWebsite(currencyCode:currencyCode, password:password);
            var registrationData = Mapper.DynamicMap<RegistrationData>(playerRegData);

            registrationData.BrandId = "00000000-0000-0000-0000-000000000138";
            registrationData.CountryCode = playerRegData.Country;
            registrationData.CurrencyCode = playerRegData.Currency;
            registrationData.CultureCode = TestDataGenerator.GetRandomCultureCode();
            registrationData.DateOfBirth =
                new DateTime(playerRegData.Year, playerRegData.Month, playerRegData.Day).ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            registrationData.MailingAddressCity = playerRegData.City;
            registrationData.MailingAddressLine1 = playerRegData.Address;
            registrationData.MailingAddressLine2 = playerRegData.AddressLine2;
            registrationData.MailingAddressLine3 = playerRegData.AddressLine3;
            registrationData.MailingAddressLine4 = playerRegData.AddressLine4;
            registrationData.MailingAddressPostalCode = playerRegData.PostalCode;
            registrationData.PasswordConfirm = playerRegData.Password;
            registrationData.PhysicalAddressCity = playerRegData.City;
            registrationData.PhysicalAddressLine1 = playerRegData.Address;
            registrationData.PhysicalAddressLine2 = playerRegData.AddressLine2;
            registrationData.PhysicalAddressLine3 = playerRegData.AddressLine3;
            registrationData.PhysicalAddressLine4 = playerRegData.AddressLine4;
            registrationData.PhysicalAddressPostalCode = playerRegData.PostalCode;
            registrationData.SecurityQuestionId = TestDataGenerator.GetRandomSecurityQuestion();

            _playerCommands.Register(registrationData);

            return playerRegData;
        }
    }
}