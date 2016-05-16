using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.ApplicationServices;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using Microsoft.Practices.ObjectBuilder2;
using Licensee = AFT.RegoV2.Core.Brand.Data.Licensee;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class BankAccountQueries : MarshalByRefObject, IApplicationService
    {
        private readonly IPaymentRepository _repository;
        private readonly BrandQueries _brandQueries;
        private readonly UserService _userService;

        public BankAccountQueries(IPaymentRepository repository, BrandQueries brandQueries, UserService userService)
        {
            _repository = repository;
            _brandQueries = brandQueries;
            _userService = userService;
        }

        public BankAccount GetBankAccount(Guid id)
        {
            return _repository.BankAccounts
                .Include(x => x.Bank)
                .Include(x => x.PaymentLevels)
                .SingleOrDefault(x => x.Id == id);
        }

        public IEnumerable<BankAccount> GetBankAccounts()
        {
            return _repository.BankAccounts
                .Include(x => x.Bank);
        }

        [Permission(Permissions.View, Module = Modules.BankAccounts)]
        public IEnumerable<BankAccount> GetFilteredBankAccounts(Guid userId, string currencyCode = null)
        {
            var filteredBankAccounts = GetFilteredBankAccountsForUser(userId);

            if (!string.IsNullOrEmpty(currencyCode))
                filteredBankAccounts = FilterBankAccountsByCurrency(filteredBankAccounts, currencyCode);

            return filteredBankAccounts;
        }

        public IEnumerable<Bank> GetBanks()
        {
            return _repository.Banks.Include(b => b.Brand);
        }

        public IEnumerable<BankAccount> GetFilteredBankAccountsForUser(Guid userId)
        {
            var filteredBrandIds = _brandQueries.GetFilteredBrands(_brandQueries.GetBrands(), userId)
                .ToArray()
                .Select(x => x.Id);

            IEnumerable<BankAccount> filteredBankAccounts = _repository.BankAccounts
                .Include(x => x.Bank.Brand)
                .Where(x => filteredBrandIds.Contains(x.Bank.BrandId));

            return filteredBankAccounts;
        }

        public IEnumerable<BankAccount> FilterBankAccountsByLicensee(IEnumerable<BankAccount> bankAccounts, Guid licenseeId)
        {
            var licenseeBrandIds = _brandQueries.GetLicensee(licenseeId)
                .Brands
                .ToArray()
                .Select(x => x.Id);

            return bankAccounts.Where(x => licenseeBrandIds.Contains(x.Bank.BrandId));
        }

        public IEnumerable<BankAccount> FilterBankAccountsByBrand(IEnumerable<BankAccount> bankAccounts, Guid brandId)
        {
            return bankAccounts.Where(x => x.Bank.BrandId == brandId);
        }

        public IEnumerable<BankAccount> FilterBankAccountsByCurrency(IEnumerable<BankAccount> bankAccounts, string currencyCode)
        {
            return bankAccounts.Where(x => x.CurrencyCode == currencyCode);
        }

        public object GetBankAccountById(Guid id)
        {
            var bankAccount = GetBankAccount(id);

            return new
            {
                BankAccount = new
                {
                    bankAccount.AccountId,
                    bankAccount.AccountName,
                    bankAccount.AccountNumber,
                    bankAccount.AccountType,
                    bankAccount.Branch,
                    bankAccount.Province,
                    bankAccount.Remarks,
                    bankAccount.CurrencyCode,
                    isAssignedToAnyPaymentLevel = bankAccount.PaymentLevels.Any()
                },
                Bank = new
                {
                    bankAccount.Bank.Id,
                    bankAccount.Bank.BrandId,
                    LicenseeId = _brandQueries.GetBrandOrNull(bankAccount.Bank.BrandId).Licensee.Id
                }
            };
        }

        public IQueryable<Licensee> GetLicensees()
        {
            var brandsWithBanks = new HashSet<Guid>(GetBanks().Select(b => b.Brand.Id)).ToArray();
            var licensees = _brandQueries.GetLicensees().Include(l => l.Brands.Select(b => b.BrandCurrencies))
                .Where(l => l.Brands.Any(b => b.BrandCurrencies.Count > 0 && brandsWithBanks.Contains(b.Id)));

            return licensees;
        }

        public IQueryable<Brand.Data.Brand> GetBrands(Guid licensee)
        {
            var brandsWithBanks = new HashSet<Guid>(GetBanks().Select(b => b.Brand.Id)).ToArray();
            var brands = _brandQueries.GetBrands()
                .Where(b => b.Licensee.Id == licensee
                            && b.BrandCurrencies.Count > 0
                            && brandsWithBanks.Contains(b.Id));

            return brands;
        }

        public IEnumerable<string> GetCurrencies(Guid brandId)
        {
            var brand = _brandQueries.GetBrands()
                .Single(b => b.Id == brandId);

            var currencies = brand.BrandCurrencies
                .Select(c => c.CurrencyCode);

            return currencies;
        }

        public IEnumerable<Bank> GetBanks(Guid brandId)
        {
            return GetBanks()
                .Where(b => b.Brand.Id == brandId);
        }

        public IEnumerable<Licensee> GetLicenseesData(Guid userId)
        {
            IEnumerable<BankAccount> bankAccounts =
                GetFilteredBankAccountsForUser(userId).ToList();

            var licensees = _brandQueries.GetLicensees();
            var licenseeSet = new HashSet<Licensee>();
            bankAccounts.ForEach(x => licenseeSet.Add(licensees.Single(y => y.Id == x.Bank.Brand.LicenseeId)));

            var licenseesData = licenseeSet.OrderBy(x => x.Name);
            return licenseesData;
        }

        public IEnumerable<Brand.Data.Brand> GetBrandsData(Guid? licenseeId, string currencyCode, Guid userId)
        {
            IEnumerable<BankAccount> bankAccounts =
                GetFilteredBankAccountsForUser(userId).ToList();

            if (licenseeId.HasValue)
                bankAccounts = FilterBankAccountsByLicensee(bankAccounts, licenseeId.Value).ToList();

            if (!string.IsNullOrEmpty(currencyCode))
                bankAccounts = FilterBankAccountsByCurrency(bankAccounts, currencyCode).ToList();

            var brands = _brandQueries.GetBrands();
            var brandSet = new HashSet<Brand.Data.Brand>();
            bankAccounts.ForEach(x => brandSet.Add(brands.Single(y => y.Id == x.Bank.Brand.Id)));

            var brandsData = brandSet.OrderBy(x => x.Name);

            return brandsData;
        }

        public IOrderedEnumerable<string> GetCurrencyData(Guid userId)
        {
            var brandFilterSelections = _userService.GetBrandFilterSelections(userId);

            var bankAccounts = GetFilteredBankAccountsForUser(userId)
                .ToList()
                .Where(x => brandFilterSelections.Contains(x.Bank.BrandId));

            var currencySet = new HashSet<string>();

            bankAccounts.ForEach(x => currencySet.Add(x.CurrencyCode));

            var currencyData = currencySet.OrderBy(x => x);

            return currencyData;
        }
    }
}
