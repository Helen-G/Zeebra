using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Content.Data;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.MemberApi.Interface.Player;
using AFT.RegoV2.Shared;
using AFT.RegoV2.Tests.Common.Pages.FrontEnd;
using Faker;

namespace AFT.RegoV2.Tests.Common
{
    public static class TestDataGenerator
    {
        private static readonly Random Rng = new Random();
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        private const string AlphatebicChars = "abcdefghijklmnopqrstuvwxyz";
        public const string NumericChars = "1234567890";
        public static string[] CurrencyCodes = { "CAD", "CNY" };
        public static string[] ProductTypes = { "Undetermined", "Sportsbook", "Casino", "Poker" };
        public static string[] CountryCodes = { "US", "CN" };
        public static string[] CountryNames = { "Canada", "United States", "Great Britain", "China" };
        public static string[] CultureCodes = { "en-US", "zh-TW" };
        public static string[] TransferTypes = { "Same Bank", "Different Bank" };
        public static string[] OfflineDepositTypes = { "Internet Banking", "ATM", "Counter Deposit" };
        public static string[] BrandTypes = { "Deposit", "Credit", "Integrated" };
        public static string[] BankAccountNumber = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
        public static string[] Amount = { "1.0", "2.0", "50.0", "51.5" };
        public static string[] Genders = Enum.GetNames(typeof(Gender));
        public static string[] Titles = Enum.GetNames(typeof(Title));
        public static string[] ContactMethods = Enum.GetNames(typeof(ContactMethod));
        public static string[] AccountStatuses = Enum.GetNames(typeof(AccountStatus));
        public static string[] PaymentTypes = { "Deposit", "Withdraw" };
        public static string[] SecurityQuestions = {
                                           "96569808-4744-4bf2-952c-86b1a634bb67", "46eb056d-72ae-4b89-bccb-f4ddf893c535", "b355b02c-4de4-4981-9a09-5de74bfc5765", 
                                           "ff621262-172b-40b7-831b-56dffe66af0b", "3be83483-8345-44bf-a0d6-7bebd61daf8d",
                                           /*
                                           "616b7039-23b5-4f50-a4a0-01160d842ff3","baba9c92-7a1c-4185-85b6-0956a8f4ac61",
                                            "7ce19eb6-9019-4787-aafe-0b7ffe15cd3b","e8c940a0-26a7-4687-a099-0e753a68160c",
                                            "af00d333-9461-48e5-bd2e-156060ebdb2b","bd979574-0664-4c59-9c1c-31dd1bfae0ff",
                                            "c5a4d536-1a11-4873-b98d-35904d525127","36a53000-6b96-4fa3-b11c-35eff2bc4d20",
                                            "0c899f88-de29-450c-bc17-40649f7e37f8","a59635c7-523d-4c74-b456-483eeb458b6d",
                                            "cffd92f3-b904-4514-8a49-778019f22a63","6995910f-2e08-4fe6-b15e-7dab6df730dd",
                                            "d4c53657-c4f9-4786-b7a2-8c841286555f","7d4e2916-1af0-4459-9951-908e2b8c672c",
                                            "b7f32b57-c933-4931-b88e-9e96e4506f13","c81c43ff-f576-4a22-9451-b52e79010d86",
                                            "2d42e434-8578-4118-b4aa-c17e921812d4","c1e23243-301f-4e92-973e-c2398816899c",
                                            "31f26887-290d-45ed-a55c-c63594b2a241","1c848cff-9822-45d0-861c-cb62e0785fcf",
                                            "30c6232a-fb05-493b-9267-f0d633bae2d2","0b6b89ea-ef52-4f9a-982e-fbbd7b1fb6f9"
                                            */
                                            };

        public static string[] Colors =
        {
            "#ffffff", "#ac725e", "#d06b64", "#f83a22", "#fa573c",
            "#ff7537", "#ffad46", "#42d692", "#16a765", "#7bd148",
            "#b3dc6c", "#fbe983", "#fad165", "#92e1c0", "#9fe1e7", 
            "#9fc6e7", "#4986e7", "#9a9cff", "#b99aff", "#c2c2c2",
            "#cabdbf", "#cca6ac", "#f691b2", "#cd74e6", "#a47ae2"
        };

        public static string GetRandomAmount()
        {
            return Amount[Rng.Next(Amount.Length)];
        }

        private static string GetRandomOf(string[] arr)
        {
            return arr[Rng.Next(arr.Length)];
        }

        public static string GetRandomSecurityQuestion()
        {
            return GetRandomOf(SecurityQuestions);
        }
        public static string GetRandomString(int size = 7, string charsToUse = Chars)
        {
            var buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = charsToUse[Rng.Next(charsToUse.Length)];
            }
            return new string(buffer);
        }

        public static string GetRandomAlphabeticString(int size, string charsToUse = AlphatebicChars)
        {
            var buffer = new char[size];

            for (int i = 0; i < size; i++)
            {
                buffer[i] = charsToUse[Rng.Next(charsToUse.Length)];
            }
            return new string(buffer);
        }

        public static string GetRandomStringWithSpecialSymbols(int size, string specialCharacters = ".-'")
        {
            if (size < specialCharacters.Length)
            {
                throw new ArgumentOutOfRangeException("size", "Size of returning string is less than special characters count");
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(GetRandomString(size, charsToUse: Chars + specialCharacters));

            //first and last character should not be from the list of special characters
            stringBuilder[0] = GetRandomString(1, charsToUse: Chars)[0];
            stringBuilder[stringBuilder.Length - 1] = GetRandomString(1, charsToUse: Chars)[0];

            //no 2 or more consequent special characters
            for (int current = 1; current < stringBuilder.Length - 1; current++)
            {
                var next = current + 1;
                var afterNext = current + 2;

                if (specialCharacters.Contains(stringBuilder[current]))
                {
                    stringBuilder[next] = GetRandomString(1, charsToUse: Chars)[0];
                    //stringBuilder[afterNext] = GetRandomString(1, charsToUse: specialCharacters)[0];
                }
            }

            return stringBuilder.ToString();
        }

        public static int GetRandomNumber(int max, int min = 1)
        {
            return Rng.Next(min, max);
        }

        public static string GetRandomCurrencyCode()
        {
            return CurrencyCodes[Rng.Next(CurrencyCodes.Length)];
        }

        public static string GetRandomTitle()
        {
            return Titles[Rng.Next(Titles.Length)];
        }

        public static string GetRandomGender()
        {
            return Genders[Rng.Next(Genders.Length)];
        }

        public static string GetRandomContactMethod()
        {
            return ContactMethods[Rng.Next(ContactMethods.Length)];
        }

        public static string GetRandomCountryCode()
        {
            return CountryCodes[Rng.Next(CountryCodes.Length)];
        }

        public static string GetRandomCountryName()
        {
            return CountryNames[Rng.Next(CountryNames.Length)];
        }

        public static string GetRandomCultureCode()
        {
            return CultureCodes[Rng.Next(CountryCodes.Length)];
        }

        public static string GetRandomColor()
        {
            return Colors[Rng.Next(Colors.Length)];
        }

        public static DateTime GetDateOfBirthOver18()
        {
            return DateTime.UtcNow.AddYears(-19);
        }

        public static string GetRandomEmail()
        {
            return string.Format("{0}@testmail.com", GetRandomString());
        }

        public static string GetRandomPhoneNumber()
        {
            var rand = new Random();
            var telNo = new StringBuilder(12);
            int number;
            for (int i = 0; i < 3; i++)
            {
                number = rand.Next(0, 8); // digit between 0 (incl) and 8 (excl)
                telNo = telNo.Append(number.ToString(CultureInfo.InvariantCulture));
            }
            telNo = telNo.Append("-");
            number = rand.Next(0, 743); // number between 0 (incl) and 743 (excl)
            telNo = telNo.Append(string.Format("{0:D3}", number));
            telNo = telNo.Append("-");
            number = rand.Next(0, 10000); // number between 0 (incl) and 10000 (excl)
            telNo = telNo.Append(string.Format("{0:D4}", number));
            return telNo.ToString();
        }

        public static RegisterRequest CreateRandomRegistrationRequestData()
        {
            var password = GetRandomString();
            return new RegisterRequest
            {
                FirstName = "FirstName" + GetRandomString(),
                LastName = "LastName" + GetRandomString(),
                Email = GetRandomEmail(),
                PhoneNumber = GetRandomPhoneNumber().Replace("-", string.Empty),
                MailingAddressLine1 = "mailing address1" + GetRandomString(),
                MailingAddressLine2 = "mailing address2",
                MailingAddressLine3 = "mailing address3",
                MailingAddressLine4 = "mailing address4",
                MailingAddressCity = GetRandomString(),
                MailingAddressPostalCode = GetRandomString(6),
                PhysicalAddressLine1 = "physical address1" + GetRandomString(),
                PhysicalAddressLine2 = "physical address2",
                PhysicalAddressLine3 = "physical address3",
                PhysicalAddressLine4 = "physical address4",
                PhysicalAddressCity = GetRandomString(),
                PhysicalAddressPostalCode = GetRandomString(6),
                CountryCode = GetRandomCountryCode(),
                CurrencyCode = GetRandomCurrencyCode(),
                CultureCode = GetRandomCultureCode(),
                Username = "Player-" + GetRandomString(5),
                Password = password,
                PasswordConfirm = password,
                DateOfBirth = GetDateOfBirthOver18().ToString("yyyy/MM/dd", CultureInfo.InvariantCulture),
                Gender = GetRandomGender(),
                Title = GetRandomTitle(),
                ContactPreference = GetRandomContactMethod(),
                IpAddress = "192.168.1.1",
                DomainName = "test.com",
                SecurityQuestionId = GetRandomSecurityQuestion(),
                SecurityAnswer = "SecurityAnswer" + GetRandomString(),
                BrandId = new Guid("00000000-0000-0000-0000-000000000138")
            };
        }

        public static RegistrationData CreateRandomRegistrationData()
        {
            var password = GetRandomString();
            return new RegistrationData
            {
                FirstName = "FirstName" + GetRandomString(),
                LastName = "LastName" + GetRandomString(),
                Email = GetRandomEmail(),
                PhoneNumber = GetRandomPhoneNumber().Replace("-", string.Empty),
                MailingAddressLine1 = "mailing address1" + GetRandomString(),
                MailingAddressLine2 = "mailing address2",
                MailingAddressLine3 = "mailing address3",
                MailingAddressLine4 = "mailing address4",
                MailingAddressCity = GetRandomString(),
                MailingAddressPostalCode = GetRandomString(6),
                PhysicalAddressLine1 = "physical address1" + GetRandomString(),
                PhysicalAddressLine2 = "physical address2",
                PhysicalAddressLine3 = "physical address3",
                PhysicalAddressLine4 = "physical address4",
                PhysicalAddressCity = GetRandomString(),
                PhysicalAddressPostalCode = GetRandomString(6),
                CountryCode = GetRandomCountryCode(),
                CurrencyCode = GetRandomCurrencyCode(),
                CultureCode = GetRandomCultureCode(),
                Username = "Player-" + GetRandomString(5),
                Password = password,
                PasswordConfirm = password,
                DateOfBirth = GetDateOfBirthOver18().ToString("yyyy/MM/dd"),
                BrandId = "00000000-0000-0000-0000-000000000138",
                Gender = GetRandomGender(),
                Title = GetRandomTitle(),
                ContactPreference = GetRandomContactMethod(),
                IpAddress = "192.168.1.1",
                DomainName = "test.com",
                SecurityQuestionId = GetRandomSecurityQuestion(),
                SecurityAnswer = "SecurityAnswer" + GetRandomString()
            };
        }

        public static string GetRandomTransferType()
        {
            return TransferTypes[Rng.Next(TransferTypes.Length)];
        }

        public static string GetRandomOfflineDepositType()
        {
            return OfflineDepositTypes[Rng.Next(OfflineDepositTypes.Length)];
        }

        public static string GetRandomBankAccountNumber(int size)
        {
            var rand = new Random();
            var bankAccountNumber = new StringBuilder(size);
            int number;
            for (int i = 0; i < 12; i++)
            {
                number = rand.Next(0, 9);
                bankAccountNumber = bankAccountNumber.Append(number.ToString(CultureInfo.InvariantCulture));
            }
            return bankAccountNumber.ToString();
        }

        public static string GetRandomPaymentType()
        {
            return PaymentTypes[Rng.Next(PaymentTypes.Length)];
        }

        public static string GetRandomIpAddress(IpVersion version = IpVersion.Ipv4)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());

            switch (version)
            {
                case IpVersion.Ipv4:
                    var address = new List<string>();

                    for (var i = 0; i < 4; i++)
                    {
                        var segmentInt = 0;
                        do
                        {
                            segmentInt = rnd.Next(1, 255);
                        } while (segmentInt == 127);

                        var segment = segmentInt.ToString();
                    
                        address.Add(segment);
                    }
                    return string.Join(".", address);
                case IpVersion.Ipv6:
                    var segments = new List<string>();

                    for (int i = 0; i < 8; i++)
                    {
                        var segment = rnd.Next(0, 65536).ToString("X");
                        segments.Add(segment);
                    }
                    return string.Join(":", segments);
                default:
                    throw new RegoException("Unrecognized IP version");
            }
        }

        public static string GetRandomIpAddressV4Range(string rangeSeparator)
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());

            var ipAddressSegments = new List<int>();

            for (var i = 0; i < 4; i++)
            {
                var segment = rnd.Next(1, 255);
                ipAddressSegments.Add(segment);
            }

            var rangeStart = ipAddressSegments.Last();
            var rangeEnd = rangeSeparator == "-"
                ? rnd.Next(rangeStart + 1, 255)
                : rnd.Next(24, 32);

            var ipAddress = string.Format("{0}{1}{2}", string.Join(".", ipAddressSegments), rangeSeparator, rangeEnd);

            return ipAddress;
        }

        public static decimal GetRandomDepositAmount()
        {
            return Rng.Next(100, 1000);
        }

        public static TimeZoneInfo GetRandomTimeZone()
        {
            var timeZones = TimeZoneInfo.GetSystemTimeZones();
            return timeZones[Rng.Next(timeZones.Count)];
        }

        public static MessageType GetRandomMessageType()
        {
            var messageTypes = (MessageType[])Enum.GetValues(typeof(MessageType));
            return messageTypes[Rng.Next(messageTypes.Length)];
        }

        public static MessageDeliveryMethod GetRandomMessageDeliveryMethod()
        {
            var messageDeliveryMethod = (MessageDeliveryMethod[])Enum.GetValues(typeof(MessageDeliveryMethod));
            return messageDeliveryMethod[Rng.Next(messageDeliveryMethod.Length)];
        }

        public static RegistrationDataForMemberWebsite CreateValidPlayerDataForMemberWebsite(
            string currencyCode = null,
            string cultureCode = null,
            string countryCode = null,
            string password = null)
        {
            var username = "Pl_" +  GetRandomStringWithSpecialSymbols(9);
            var result = new RegistrationDataForMemberWebsite
            {
                Username = username,
                Password = password ?? username,
                Title =  GetRandomTitle(),
                FirstName = "First-name" +  GetRandomStringWithSpecialSymbols(36),
                LastName = "Last-name" +  GetRandomStringWithSpecialSymbols(11),
                Gender =  GetRandomGender(),
                Email =  GetRandomString(11) + "@mail.ru",
                PhoneNumber =  GetRandomPhoneNumber().Replace("-", string.Empty),
                Day = 10,
                Month = 12,
                Year =  GetRandomNumber(1997, 1914),
                Country = countryCode ??  GetRandomCountryCode(),
                Currency = currencyCode ??  GetRandomCurrencyCode(),
                Address =  GetRandomString(50),
                AddressLine2 = "address Line 2",
                AddressLine3 = "address Line 3",
                AddressLine4 = "address Line 4",
                City = "Singapore",
                PostalCode =  GetRandomString(10),
                ContactPreference =  GetRandomContactMethod(),
                SecurityQuestion =  GetRandomSecurityQuestion(),
                SecurityAnswer = "SecurityAnswer" +  GetRandomString()
            };

            return result;
        }

        public static RegistrationDataForMemberWebsite CreateRegistrationDataWithSpacesOnly()
        {
            const string data = "   ";
            var result = new RegistrationDataForMemberWebsite
            {
                Username = data,
                FirstName = data,
                LastName = data,
                Email = data,
                PhoneNumber = data,
                Password = data,
                Address = data,
                PostalCode = data,
                City = data,
                Gender = data,
                Title = data,
                AddressLine2 = data,
                AddressLine3 = data,
                AddressLine4 = data,
                ContactPreference = data,
                SecurityAnswer = data,
            };

            return result;
        }

        public static PlayerRegistrationDataForAdminWebsite CreateValidPlayerDataForAdminWebsite(
            string licenseeName,
            string brandName,
            string currency = null,
            string culture = null,
            string country = null)
        {
            var username = "Player_" +  GetRandomStringWithSpecialSymbols(5);

            var result = new PlayerRegistrationDataForAdminWebsite
            {
                Licensee = licenseeName,
                Brand = brandName,
                LoginName = username,
                Password = username,
                ConfirmPassword = username,
                Gender =  GetRandomGender(),
                Title =  GetRandomTitle(),
                FirstName = "First-name" +  GetRandomStringWithSpecialSymbols(36),
                LastName = "Last-name" +  GetRandomStringWithSpecialSymbols(11),
                Email =  GetRandomString(11) + "@mail.ru",
                MobileNumber =  GetRandomPhoneNumber().Replace("-", string.Empty),
                Country = country ??  GetRandomCountryCode(),
                Currency = currency ??  GetRandomCurrencyCode(),
                Culture = culture ??  GetRandomCultureCode(),
                AffiliateCode =  GetRandomString(6),
                Address =  GetRandomString(50),
                AddressLine2 = Address.SecondaryAddress(),
                AddressLine3 = "Address Line 3 " +  GetRandomString(10),
                AddressLine4 = "Address Line 4 " +  GetRandomString(10),
                City = Address.City(),
                ZipCode =  GetRandomString(10),
                ContactPreference =  GetRandomContactMethod(),
                SecurityQuestion =  GetRandomSecurityQuestion(),
                SecurityAnswer = "SecurityAnswer" +  GetRandomString(),
                AccountStatus = AccountStatus.Active.ToString(),
                AccountAlertEmail = true,
                AccountAlertSms = true
            };

            return result;
        }

        public static PaymentSettingsData CreateValidPaymentSettingsData()
        {
            var currencyCode =  GetRandomCurrencyCode();
            var result = new PaymentSettingsData
            {
                Licensee = "Flycow",
                Brand = "138",
                Currency = currencyCode,
                PaymentType =  GetRandomPaymentType(),
                MinAmountPerTransaction = "10",
                MaxAmountPerTransaction = "100",
                MaxAmountPerDay = "1",
                MaxTransactionsPerDay = "1",
                MaxTransactionsPerWeek = "1",
                MaxTransactionsPerMonth = "1"
            };

            if (currencyCode == "CAD")
            {
                result.PaymentMethod = "Offline - CWB1";
            }
            else
            {
                result.PaymentMethod = "Offline - HXB1";
            }
            return result;
        }

        public static RoleData CreateValidRoleData(string code, string name, string licensee, string description = null)
        {
            var result = new RoleData
            {
                RoleCode = code ?? GetRandomString(6),
                RoleName = name ?? "User" + GetRandomString(14),
                Licensee = licensee ?? "Flycow",
                Description = description ?? "Description" + GetRandomString(6),
            };
            return result;
        }

        public static BankAccountData EditBankAccountData()
        {
            var result = new BankAccountData
            {
                ID =  GetRandomString(6),
                Name =  GetRandomString(6),
                Number =  GetRandomBankAccountNumber(10),
                Type = "simple",
                Province =  GetRandomString(6),
                Branch = "main",
                Remarks =  GetRandomString(20)
            };
            return result;
        }

        public static PaymentSettingsData CreateValidPaymentSettingsDataForDefaultLicensee(string licensee,
            string brand, string currencyCode, string minAmountPerTrans, string maxAmountPerTrans)
        {
            var result = new PaymentSettingsData
            {
                Licensee = licensee,
                Brand = brand,
                Currency = currencyCode,
                PaymentType =  GetRandomPaymentType(),
                MinAmountPerTransaction = minAmountPerTrans,
                MaxAmountPerTransaction = maxAmountPerTrans,
                MaxAmountPerDay = "1",
                MaxTransactionsPerDay = "1",
                MaxTransactionsPerWeek = "1",
                MaxTransactionsPerMonth = "1"
            };

            if (currencyCode == "CAD")
            {
                result.PaymentMethod = "Offline - CWB1";
            }
            else
            {
                result.PaymentMethod = "Offline - HXB1";
            }
            return result;
        }

        public static PaymentSettingsData CreateValidPaymentSettingsData(string licensee,
             string brand, string currencyCode, string paymentMetod, string paymentType, string vipLevel,
             string minAmountPerTrans, string maxAmountPerTrans,
             string maxAmountPerDay, string maxTransactionsPerDay,
             string maxTransactionsPerWeek, string maxTransactionsPerMonth)
        {
            var result = new PaymentSettingsData
            {
                Licensee = licensee,
                Brand = brand,
                Currency = currencyCode,
                PaymentType = paymentType,
                PaymentMethod = paymentMetod,
                VipLevel = vipLevel,
                MinAmountPerTransaction = minAmountPerTrans,
                MaxAmountPerTransaction = maxAmountPerTrans,
                MaxAmountPerDay = maxAmountPerDay,
                MaxTransactionsPerDay = maxTransactionsPerDay,
                MaxTransactionsPerWeek = maxTransactionsPerWeek,
                MaxTransactionsPerMonth = maxTransactionsPerMonth
            };

            return result;
        }

        public static AutoVerificationConfigurationData CreateAutoVerificationConfigurationData(string licensee,
        string brand, string currencyCode, string vipLevel)
        {
            var result = new AutoVerificationConfigurationData
            {
                Licensee = licensee,
                Brand = brand,
                Currency = currencyCode,
                VipLevel = vipLevel,
             };

            return result;
        }

        public static TransferSettingsData CreateValidTransferSettingsData(string licensee,
             string brand, string currencyCode, string productWallet, string transferFundType, string vipLevel,
             string minAmountPerTrans, string maxAmountPerTrans,
             string maxAmountPerDay, string maxTransactionsPerDay,
             string maxTransactionsPerWeek, string maxTransactionsPerMonth)
        {
            var result = new TransferSettingsData
            {
                Licensee = licensee,
                Brand = brand,
                Currency = currencyCode,
                ProductWallet = productWallet,
                TransferFundType = transferFundType,
                VipLevel = vipLevel,
                MinAmountPerTransaction = minAmountPerTrans,
                MaxAmountPerTransaction = maxAmountPerTrans,
                MaxAmountPerDay = maxAmountPerDay,
                MaxTransactionsPerDay = maxTransactionsPerDay,
                MaxTransactionsPerWeek = maxTransactionsPerWeek,
                MaxTransactionsPerMonth = maxTransactionsPerMonth
            };

            return result;
        }

        public static RegistrationDataForMemberWebsite RegistrationDataExceedsMaxLimit()
        {
            var username = "Pl_" +  GetRandomStringWithSpecialSymbols(10);
            var result = new RegistrationDataForMemberWebsite
            {
                Username = username,
                FirstName = "First-name" +  GetRandomStringWithSpecialSymbols(37),
                LastName = "Last-name" +  GetRandomStringWithSpecialSymbols(12),
                Email =  GetRandomString(43) + "@mail.ru",
                PhoneNumber =  GetRandomPhoneNumber().Replace("-", string.Empty),
                Password = username,
                Address =  GetRandomString(51),
                PostalCode =  GetRandomString(11),
                Country =  GetRandomCountryCode(),
                Currency =  GetRandomCurrencyCode(),
                Gender =  GetRandomGender(),
                Title =  GetRandomTitle(),
                AddressLine2 = "address Line 2",
                AddressLine3 = "address Line 3",
                AddressLine4 = "address Line 4",
                City = "Singapore",
                ContactPreference =  GetRandomContactMethod(),
            };
            return result;
        }

        public static DepositConfirmRegistrationData CreateValidDepositConfirmData(string playerFullName)
        {
            var result = new DepositConfirmRegistrationData()
            {
                PlayerAccountName = playerFullName,
                PlayerAccountNumber =  GetRandomString(15),
                BankReferenceNumber =  GetRandomString(15),
                Amount = 100.25M,
                TransferType =  GetRandomTransferType(),
                OfflineDepositType =  GetRandomOfflineDepositType(),
                Remarks = "confirmed deposit"
            };
            return result;
        }

        public static DepositConfirmRegistrationData CreateValidDepositConfirmData(string playerFullName, decimal amount)
        {
            var result = new DepositConfirmRegistrationData()
            {
                PlayerAccountName = playerFullName,
                PlayerAccountNumber =  GetRandomString(15),
                BankReferenceNumber =  GetRandomString(15),
                Amount = amount,
                TransferType =  GetRandomTransferType(),
                OfflineDepositType =  GetRandomOfflineDepositType(),
                Remarks = "confirmed deposit"
            };
            return result;
        }

        public static AdminUserRegistrationData CreateValidAdminUserRegistrationData(
            string role, string status, string licensee, string brand, string currency)
        {
            var userName = "User" +  GetRandomString(5);
            var result = new AdminUserRegistrationData()
            {
                UserName = userName,
                Password = userName,
                RepeatPassword = userName,
                Role = role,
                FirstName = "First-name" +  GetRandomString(5),
                LastName = "Last-name" +  GetRandomString(5),
                Status = status,
                Licensee = licensee,
                Brand = brand,
                Currency = currency,
                Description = "Description" +  GetRandomString(5),
            };
            return result;
        }

        public static AdminUserRegistrationData CreateAdminUserRegistrationDataExceedsMaxLimit(
            string role, string status, string licensee, string brand, string currency)
        {
            var password =  GetRandomString(51);
            var result = new AdminUserRegistrationData()
            {
                UserName = "User" +  GetRandomString(51),
                Password = password,
                RepeatPassword = password,
                Role = role,
                FirstName =  GetRandomString(51),
                LastName =  GetRandomString(51),
                Status = status,
                Description =  GetRandomString(201),
                Licensee = licensee,
                Brand = brand,
                Currency = currency
            };
            return result;
        }

        public static PlayerBankAccountData CreatePlayerBankAccountData()
        {
            var result = new PlayerBankAccountData
            {
                BankName = "Bank-" +  GetRandomString(5),
                Province = "Province" +  GetRandomString(5),
                City = "City" +  GetRandomString(5),
                Branch = "Branch" +  GetRandomString(5),
                SwiftCode = "Swift" +  GetRandomString(4),
                Address = "Address" +  GetRandomString(5),
                BankAccountName = "Bank Account" +  GetRandomString(5),
                BankAccountNumber =  GetRandomBankAccountNumber(10)
            };
            return result;
        }

        public static PlayerBankAccountData CreatePlayerBankAccountData(
            string bankName,
            string province,
            string city,
            string branch,
            string swiftCode,
            string address,
            string bankAccountName,
            string bankAccountNumber)
        {
            var result = new PlayerBankAccountData
            {
                BankName = bankName,
                Province = province,
                City = city,
                Branch = branch,
                SwiftCode = swiftCode,
                Address = address,
                BankAccountName = bankAccountName,
                BankAccountNumber = bankAccountNumber
            };
            return result;
        }

        public static OfflineWithdrawRequestData CreateOfflineWithdrawRequestData(
            string amount,
            string confirmAmount
            )
        {
            return CreateOfflineWithdrawRequestData(amount, confirmAmount, "Remark" +  GetRandomString(15));
        }

        public static OfflineWithdrawRequestData CreateOfflineWithdrawRequestData(
            string amount,
            string confirmAmount,
            string remarks
            )
        {
            var result = new OfflineWithdrawRequestData
            {
                Amount = amount,
                ConfirmAmount = confirmAmount,
                Remarks = remarks
            };
            return result;
        }

        public static AdminUserRegistrationData EditAdminUserData(
            string username,
            string role,
            string firstName,
            string lastName,
            string status,
            string licensee,
            string brand,
            string currency,
            string description)
        {
            var result = new AdminUserRegistrationData()
            {
                UserName = username + 2,
                Role = role,
                FirstName = firstName + 2,
                LastName = lastName + 2,
                Status = status,
                Licensee = licensee,
                Brand = brand,
                Currency = currency,
                Description = description + 2,
            };
            return result;
        }


        public static LicenseeData EditLicenseeData(
            string licenseeName, 
            string companyName, 
            string contractStart, 
            string email,
            string contractEnd = null,
            string[] availableProducts = null,
            string[] availableCurrencies = null,
            string[] availableCountries = null,
            string[] availableLanguages = null,
            string timezone = null
            )
        {
            var result = new LicenseeData()
            {
                LicenseeName = licenseeName,
                CompanyName = companyName,
                ContractStart = contractStart,
                ContractEnd = contractEnd,
                Email = email,
                Timezone = timezone,
                NumberOfAllowedBrands =  GetRandomNumber(10).ToString(),
                NumberOfAllowedWebsitesPerBrand =  GetRandomNumber(10).ToString(),
                AvailableProducts = availableProducts,
                AvailableCurrencies = availableCurrencies,
                AvailableCountries = availableCountries,
                AvailableLanguages = availableLanguages,
                Remarks =  GetRandomString(10)
            };
            return result;
        }

        public static LanguageData CreateLanguageData()
        {
            var result = new LanguageData()
            {
                LanguageCode =  GetRandomString(3),
                LanguageName =  GetRandomString(3),
                NaviteName = "Name" +  GetRandomString(5)
            };
            return result;
        }

        public static VipLevelData CreateValidVipLevelData(
            string licensee,
            string brand,
            bool defaultForNewPlayers = true,
            string displayColor = null)
        {
            var result = new VipLevelData
            {
                Licensee = licensee,
                Brand = brand,
                DefaultForNewPlayers = defaultForNewPlayers,
                Code =  GetRandomString(5),
                Name =  GetRandomString(5),
                Rank =  GetRandomNumber(1000),
                Description =  GetRandomString(50),
                DisplayColor = displayColor ?? GetRandomColor()
            };

            return result;
        }
    }

    public class VipLevelData
    {
        public string Licensee;
        public string Brand;
        public bool DefaultForNewPlayers;
        public string Code;
        public string Name;
        public int Rank;
        public string Description;
        public string DisplayColor;
    }

    public class BankAccountData
    {
        public string ID;
        public string Name;
        public string Number;
        public string Type;
        public string Province;
        public string Branch;
        public string Remarks;
    }

    public class LanguageData
    {
        public string LanguageCode;
        public string LanguageName;
        public string NaviteName;
    }

    public class RoleData
    {
        public string RoleCode;
        public string RoleName;
        public string Licensee;
        public string Description;
    }

    public class PlayerBankAccountData
    {
        public string BankName;
        public string Province;
        public string City;
        public string Branch;
        public string SwiftCode;
        public string Address;
        public string BankAccountName;
        public string BankAccountNumber;
    }

    public class OfflineWithdrawRequestData
    {
        public string Amount;
        public string ConfirmAmount;
        public string Remarks;
    }

    public class PlayerRegistrationDataForAdminWebsite
    {
        public string Licensee;
        public string Brand;
        public string LoginName;
        public string FirstName;
        public string LastName;

        public string FullName
        {
            get { return this.FirstName + " " + this.LastName; }
        }

        public string Email;
        public string MobileNumber;
        public string Password;
        public string ConfirmPassword;
        public string Address;
        public string ZipCode;
        public string Country;
        public string Currency;
        public string Culture;
        public string PaymentLevel;
        public string Gender;
        public string Title;
        public string AddressLine2;
        public string AddressLine3;
        public string AddressLine4;
        public string City;
        public string ContactPreference;
        public string Comments;
        public string DateOfBirth;
        public string SecurityQuestion;
        public string SecurityAnswer;
        public string AffiliateCode;
        public string AccountStatus;
        public bool AccountAlertEmail;
        public bool AccountAlertSms;
    }

    public class PaymentSettingsData
    {
        public string Licensee;
        public string Brand;
        public string Currency;
        public string PaymentType;
        public string PaymentMethod;
        public string VipLevel;
        public string MinAmountPerTransaction;
        public string MaxAmountPerTransaction;
        public string MaxAmountPerDay;
        public string MaxTransactionsPerDay;
        public string MaxTransactionsPerWeek;
        public string MaxTransactionsPerMonth;
    }

    public class AutoVerificationConfigurationData
    {
        public string Licensee;
        public string Brand;
        public string Currency;
        public string VipLevel;
    }

    public class TransferSettingsData
    {
        public string Licensee;
        public string Brand;
        public string Currency;
        public string ProductWallet;
        public string TransferFundType;
        public string VipLevel;
        public string MinAmountPerTransaction;
        public string MaxAmountPerTransaction;
        public string MaxAmountPerDay;
        public string MaxTransactionsPerDay;
        public string MaxTransactionsPerWeek;
        public string MaxTransactionsPerMonth;
    }

    public class DepositConfirmRegistrationData
    {
        public string PlayerAccountName;
        public string PlayerAccountNumber;
        public string BankReferenceNumber;
        public decimal Amount;
        public string TransferType;
        public string OfflineDepositType;
        public string Remarks;
    }

    public class AdminUserRegistrationData
    {
        public string UserName;
        public string Password;
        public string RepeatPassword;
        public string Role;
        public string FirstName;
        public string LastName;
        public string Language;
        public string Status;
        public string Description;
        public string Licensee;
        public string Brand;
        public string Currency;
    }

    public class LicenseeData
    {
        public string LicenseeName;
        public string CompanyName;
        public bool AffiliateSystem;
        public string ContractStart;
        public string ContractEnd;
        public string OpenEnded;
        public string Email;
        public string Timezone;
        public string NumberOfAllowedBrands;
        public string NumberOfAllowedWebsitesPerBrand;
        public string[] AvailableProducts = null;
        public string[] AvailableCurrencies = null;
        public string[] AvailableCountries = null;
        public string[] AvailableLanguages = null;
        public string Remarks;
    }

    public class PaymentSettingsValues
    {
        public decimal MinAmountPerTransaction;
        public decimal MaxAmountPerTransaction;
        public decimal MaxAmountPerDay;
        public int MaxTransactionsPerDay;
        public int MaxTransactionsPerWeek;
        public int MaxTransactionsPerMonth;
    }


    public enum IpVersion
    {
        Ipv4,
        Ipv6
    }
}
