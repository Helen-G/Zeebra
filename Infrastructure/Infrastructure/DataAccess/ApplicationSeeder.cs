using System;
using System.Globalization;
using System.Linq;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Validators;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Payment.ApplicationServices;

namespace AFT.RegoV2.Infrastructure.DataAccess
{
    public class ApplicationSeeder 
    {
        private readonly IBrandRepository   _brandRepository;
        private readonly ILicenseeCommands  _licenseeCommands;
        private readonly ICurrencyCommands  _currencyCommands;
        private readonly IBrandCommands     _brandCommands;
        private readonly ICultureCommands   _cultureCommands;

        public ApplicationSeeder(
            IBrandRepository repository, 
            ILicenseeCommands licenseeCommands,
            ICurrencyCommands currencyCommands,
            IBrandCommands brandCommands,
            ICultureCommands cultureCommands)
        {
            _brandRepository = repository;
            _licenseeCommands = licenseeCommands;
            _currencyCommands = currencyCommands;
            _brandCommands = brandCommands;
            _cultureCommands = cultureCommands;
        }

        public void Seed()
        {
            AddCurrencies(new[] { "CNY", "CAD", "UAH", "EUR", "GBP", "USD", "RUB", "ALL", "BDT", "ZAR" });

            AddCountry("US", "United States");
            AddCountry("CA", "Canada");
            AddCountry("GB", "Great Britain");
            AddCountry("CN", "China");

            AddCultureCode("en-US", "English US", "English");
            AddCultureCode("en-GB", "English UK", "English");
            AddCultureCode("zh-CN", "Chinese Simplified", "Chinese");
            AddCultureCode("zh-TW", "Chinese Traditional", "Chinese");

            var licenseeId = new Guid("4A557EA9-E6B7-4F1F-AEE5-49E170ADB7E0");
            AddLicensee(licenseeId, "Flycow", "Flycow Inc.", "flycow@flycow.com");
        }

        private void AddCultureCode(string code, string name, string nativeName)
        {
            if (_brandRepository.Cultures.Any(c => c.Code == code)) 
                return;

            _cultureCommands.Save(new EditCultureData { Code = code, Name = name, NativeName = nativeName});
        }

        private void AddCurrencies(string[] codes)
        {
            var regionInfoGroups = CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Where(c => !c.IsNeutralCulture)
                .Select(culture =>
                {
                    const int invariantClutureId = 127;
                    if (culture.LCID == invariantClutureId)
                        return null;
                    return new RegionInfo(culture.LCID);
                })
                .Where(ri => ri != null)
                .GroupBy(ri => ri.ISOCurrencySymbol)
                .Where(x => codes.Contains(x.Key))
                .ToArray();

            foreach (var group in regionInfoGroups)
            {
                var code = group.Key;
                if (_brandRepository.Currencies.Any(x => x.Code == code))
                    continue;

                _currencyCommands.Add(new EditCurrencyData
                {
                    Code = code,
                    Name = group.First().CurrencyEnglishName,
                    Remarks = "Created automatically while seeding database at first start"
                });
                _brandRepository.SaveChanges();
            }
        }

        private void AddCountry(string code, string name)
        {
            if (_brandRepository.Countries.Any(c => c.Code == code))
                return;

            _brandCommands.CreateCountry(code, name);
        }

        private void AddLicensee(Guid licenseeId, string name, string companyName, string email)
        {
            if (_brandRepository.Licensees.Any(b => b.Name == name))
                return;

            var contractStartDate = DateTimeOffset.ParseExact(
                DateTimeOffset.UtcNow.ToString("yyyy'/'MM'/'dd"), 
                "yyyy/MM/dd", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.AssumeUniversal
            );

            _licenseeCommands.Add(new AddLicenseeData
            {
                Id = licenseeId,
                Name = name,
                CompanyName = companyName,
                Email = email,
                ContractStart = contractStartDate,
                ContractEnd = contractStartDate.AddMonths(1),
                TimeZoneId = "Pacific Standard Time",
                BrandCount = 10,
                WebsiteCount = 10,

               // Products = new[] { Guid.NewGuid().ToString() },
                Countries = _brandRepository.Countries.Select(c => c.Code).ToArray(),
                Currencies = _brandRepository.Currencies.Select(c => c.Code).ToArray(),
                Languages = _brandRepository.Cultures.Select(c => c.Code).ToArray()
            });

            _licenseeCommands.Activate(licenseeId, "Activated when database has been seeded on first application start");
        }
    }
}