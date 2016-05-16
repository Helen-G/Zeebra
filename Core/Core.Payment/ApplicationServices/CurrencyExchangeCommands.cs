using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{

    public interface ICurrencyExchangeCommands : IApplicationService
    {
        string Add(SaveCurrencyExchangeData model);
        string Save(SaveCurrencyExchangeData model);
        string Revert(SaveCurrencyExchangeData model);
    }
    
    public class CurrencyExchangeCommands : ICurrencyExchangeCommands
    {
        private readonly ISecurityProvider _securityProvider;
        private readonly IEventBus _eventBus;

        private readonly IPaymentRepository _paymentRepository;
        private readonly PaymentQueries _queries;

        public CurrencyExchangeCommands(
            ISecurityProvider securityProvider,
            IEventBus eventBus,
            IPaymentRepository paymentRepository,
            PaymentQueries queries
            )
        {
            _queries = queries;
            _securityProvider = securityProvider;
            _eventBus = eventBus;
            _paymentRepository = paymentRepository;
        }
        
        public string Add(SaveCurrencyExchangeData model)
        {
            string message;

            var username = _securityProvider.IsUserAvailable ? _securityProvider.User.UserName : "System";

            if (_queries.GetCurrencyExchanges().Any(c => c.Brand.Id == model.BrandId && c.IsBaseCurrency && c.Brand.BaseCurrencyCode == model.Currency))
            {
                throw new RegoException("Base currency can't set as Currency To");
            }

            if (_queries.GetCurrencyExchanges().Any(c => c.BrandId == model.BrandId &&  !c.IsBaseCurrency && c.CurrencyToCode == model.Currency))
            {
                throw new RegoException("Currency Exchange already exist");
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var currencyExchange = new CurrencyExchange()
                {
                    BrandId = model.BrandId,
                    CurrencyToCode = model.Currency,
                    CurrentRate = model.CurrentRate,
                    CreatedBy = username,
                    DateCreated = DateTimeOffset.UtcNow,
                };

                _paymentRepository.CurrencyExchanges.Add(currencyExchange);
                _paymentRepository.SaveChanges();
                
                scope.Complete();

                message = "app:currencies.created";
            }

            return message;
        }

        public string Save(SaveCurrencyExchangeData model)
        {
            string message;

            var username = _securityProvider.IsUserAvailable ? _securityProvider.User.UserName : "System";

            var currencyExchange = _paymentRepository.CurrencyExchanges.SingleOrDefault(c => c.Brand.Id == model.BrandId && c.CurrencyTo.Code == model.Currency);
            if (currencyExchange == null)
            {
                throw new RegoException("Currency Exchange not found");
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                currencyExchange.PreviousRate = currencyExchange.CurrentRate;
                currencyExchange.CurrentRate = model.CurrentRate;
                currencyExchange.DateUpdated = DateTimeOffset.UtcNow;
                currencyExchange.UpdatedBy = username;

                _paymentRepository.SaveChanges();

                scope.Complete();

                message = "app:currencies.updated";
            }

            return message;
        }

        public string Revert(SaveCurrencyExchangeData model)
        {
            string message;

            var username = _securityProvider.IsUserAvailable ? _securityProvider.User.UserName : "System";
            
            var currencyExchange = _paymentRepository.CurrencyExchanges.SingleOrDefault(c => c.Brand.Id == model.BrandId && c.CurrencyTo.Code == model.Currency);
            if (currencyExchange == null)
            {
                throw new RegoException("Currency Exchange not found");
            }
            
            if (currencyExchange.PreviousRate == null)
            {
                throw new RegoException("Currency Exchange Previous Rate not found");
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                currencyExchange.PreviousRate = currencyExchange.CurrentRate;
                currencyExchange.CurrentRate = model.PreviousRate;
                currencyExchange.DateUpdated = DateTimeOffset.UtcNow;
                currencyExchange.UpdatedBy = username;

                _paymentRepository.SaveChanges();

                scope.Complete();

                message = "app:currencies.updated";
            }

            return message;
        }
    }

    public class SaveCurrencyExchangeData
    {
        public Guid BrandId { get; set; }
        public string Currency { get; set; }
        public decimal CurrentRate { get; set; }
        public decimal PreviousRate { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset DateCreated { get; set; }
    }
}
