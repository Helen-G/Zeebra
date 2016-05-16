using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Data.Commands;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class PaymentLevelQueries : MarshalByRefObject, IApplicationService
    {
        private readonly IPaymentRepository _repository;
        private readonly PaymentQueries _paymentQueries;
        private readonly IBrandRepository _brandRepository;
        private readonly PlayerQueries _playerQueries;
        private readonly BrandQueries _brandQueries;

        public PaymentLevelQueries(
            IPaymentRepository repository, 
            PaymentQueries paymentQueries, 
            IBrandRepository brandRepository, 
            PlayerQueries playerQueries,
            BrandQueries _brandQueries)
        {
            _repository = repository;
            _paymentQueries = paymentQueries;
            _brandRepository = brandRepository;
            _playerQueries = playerQueries;
            this._brandQueries = _brandQueries;
        }

        public object GetBrandCurrencies(Guid brandId)
        {
            var brand = _brandRepository.Brands
                .Include(b => b.BrandCurrencies)
                .Single(b => b.Id == brandId);

            return brand.BrandCurrencies.Select(c => c.CurrencyCode);
        }

        public PaymentLevelTransferObj GetPaymentLevelById(Guid id)
        {
            var level = _paymentQueries.GetPaymentLevel(id);
            var defaultPaymentLevelId = _brandQueries.GetDefaultPaymentLevelId(level.BrandId, level.CurrencyCode);

            var obj = new PaymentLevelTransferObj
            {
                Brand = new
                {
                    Id = level.Brand.Id,
                    Name = level.Brand.Name,
                    Licensee = new
                    {
                        Id = level.Brand.LicenseeId,
                        Name = level.Brand.LicenseeName
                    }
                },
                Currency = level.CurrencyCode,
                Code = level.Code,
                Name = level.Name,
                EnableOfflineDeposit = level.EnableOfflineDeposit,
                IsDefault = defaultPaymentLevelId == level.Id,
                BankAccounts = level.BankAccounts.Select(x => x.Id)
            };

            return obj;
        }

        public IQueryable<PaymentLevel> GetReplacementPaymentLevels(Guid id)
        {
            var paymentLevel = _repository.PaymentLevels.Single(x => x.Id == id);

            var replacementPaymentLevels = _repository.PaymentLevels
                .Where(x => 
                    x.Id != id &&
                    x.BrandId == paymentLevel.BrandId &&
                    x.CurrencyCode == paymentLevel.CurrencyCode &&
                    x.Status == PaymentLevelStatus.Active)
                .OrderBy(x => x.Name);

            return replacementPaymentLevels;
        }

        public DeactivatePaymentLevelStatus GetDeactivatePaymentLevelStatus(Guid id)
        {
            var paymentLevel = _repository.PaymentLevels.Single(x => x.Id == id);

            if (paymentLevel.Status == PaymentLevelStatus.Inactive) 
                return DeactivatePaymentLevelStatus.CannotDeactivateStatusInactive;

            var isInUse = _playerQueries.GetPlayersByPaymentLevel(id).Any();
            var defaultPaymentLevelId = _brandQueries.GetDefaultPaymentLevelId(paymentLevel.BrandId, paymentLevel.CurrencyCode);
            var replacementRequired = paymentLevel.Id == defaultPaymentLevelId || isInUse;

            if (!replacementRequired)
                return DeactivatePaymentLevelStatus.CanDeactivate;

            var isReplacementAvailable = _repository.PaymentLevels.Any(x =>
                x.Id != id &&
                x.BrandId == paymentLevel.BrandId &&
                x.CurrencyCode == paymentLevel.CurrencyCode &&
                x.Status == PaymentLevelStatus.Active);

            if (!isReplacementAvailable)
                return DeactivatePaymentLevelStatus.CannotDeactivateNoReplacement;

            return paymentLevel.Id == defaultPaymentLevelId
                ? DeactivatePaymentLevelStatus.CanDeactivateIsDefault
                : DeactivatePaymentLevelStatus.CanDeactivateIsAssigned;
        }

        public IQueryable<PaymentLevel> GetPaymentLevelsByBrandAndCurrency(Guid brandId, string currencyCode)
        {
            var paymentLevels = _repository.PaymentLevels
                .Where(x => x.BrandId == brandId && x.CurrencyCode == currencyCode)
                    .Include(x => x.Brand);

            return paymentLevels;
        }
    }

    public class PaymentLevelTransferObj
    {
        public object Brand { get; set; }
        public string Currency { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool EnableOfflineDeposit { get; set; }
        public bool IsDefault { get; set; }
        public IEnumerable<Guid> BankAccounts { get; set; }
    }

    public class PaymentLevelSaveResult
    {
        public string Message { get; set; }
        public Guid PaymentLevelId { get; set; }
    }

    public class EditPaymentLevel
    {
        public Guid? Id { get; set; }
        public Guid? Brand { get; set; }
        public string Currency { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool EnableOfflineDeposit { get; set; }
        public bool IsDefault { get; set; }
        public string[] BankAccounts { get; set; }
    }
}
