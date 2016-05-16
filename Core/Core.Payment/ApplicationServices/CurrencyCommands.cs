using System;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Payment;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Admin;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public interface ICurrencyCommands : IApplicationService
    {
        string Add(EditCurrencyData model);
        string Save(EditCurrencyData model);
    }

    public class CurrencyCommands : ICurrencyCommands
    {
        private readonly IUserInfoProvider _userInfoProvider;
        private readonly IEventBus _eventBus;

        private readonly IPaymentRepository _paymentRepository;
        private readonly PaymentQueries _queries;

        public CurrencyCommands(
            IUserInfoProvider userInfoProvider,
            IEventBus eventBus,
            IPaymentRepository paymentRepository,
            PaymentQueries queries
            )
        {
            _queries = queries;
            _userInfoProvider = userInfoProvider;
            _eventBus = eventBus;
            _paymentRepository = paymentRepository;
        }

        [Permission(Permissions.Add, Module = Modules.CurrencyManager)]
        public string Add(EditCurrencyData model)
        {
            string message;

            if (_queries.GetCurrencies().Any(c => c.Code == model.Code ))
            {
                throw new RegoException("app:common.codeUnique");
            }

            if (_queries.GetCurrencies().Any(c => c.Name == model.Name ))
            {
                throw new RegoException("app:common.nameUnique");
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var username = _userInfoProvider.User.Username;
                var currency = new Currency()
                {
                    Code = model.Code,
                    CreatedBy = username,
                    DateCreated = DateTimeOffset.UtcNow,
                    Name = model.Name,
                    Remarks = model.Remarks
                };
                
                _paymentRepository.Currencies.Add(currency);
                _paymentRepository.SaveChanges();

                var currencyCreated = new CurrencyCreated
                {
                    Code = currency.Code,
                    Name = currency.Name,
                    Remarks = currency.Remarks,
                    Status = currency.Status,
                    CreatedBy = currency.CreatedBy,
                    DateCreated = currency.DateCreated
                };

                _eventBus.Publish(currencyCreated);

                scope.Complete();

                message = "app:currencies.created";
            }

            return message;
        }

        [Permission(Permissions.Edit, Module = Modules.CurrencyManager)]
        public string Save(EditCurrencyData model)
        {
            var oldCode = model.OldCode;
            var oldName = model.OldName;

            string messageName;
            
            var currency = _paymentRepository.Currencies.SingleOrDefault(c => c.Code == oldCode);
            if (currency == null)
            {
                throw new RegoException("app:common.invalidId");
            }
            
            if (_queries.GetCurrencies().Any(c => c.Code == model.Code && c.Code != oldCode))
            {
                throw new RegoException("app:common.codeUnique");
            }

            if (_queries.GetCurrencies().Any(c => c.Name == model.Name && c.Name != oldName))
            {
                throw new RegoException("app:common.nameUnique");
            }
            
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var username = _userInfoProvider.User.Username;
                currency.UpdatedBy = username;
                currency.DateUpdated = DateTimeOffset.UtcNow;
                currency.Name = model.Name;
                currency.Remarks = model.Remarks;

                _paymentRepository.SaveChanges();
                
                var currencyUpdated = new CurrencyUpdated
                {
                    OldCode = model.OldCode,
                    OldName = model.OldName,
                    Code = currency.Code,
                    Name = currency.Name,
                    Remarks = currency.Remarks,
                    DateUpdated = currency.DateUpdated.Value,
                    UpdatedBy = currency.UpdatedBy
                };
                    
                _eventBus.Publish(currencyUpdated);
                
                scope.Complete();

                messageName = "app:currencies.updated";
            }

            return messageName;
        }

        
    }
}