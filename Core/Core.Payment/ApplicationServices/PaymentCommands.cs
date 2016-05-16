using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Domain.Payment;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class PaymentCommands : MarshalByRefObject, IApplicationService
    {
        private readonly IPaymentRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly IUserInfoProvider _userInfoProvider;

        public PaymentCommands(
            IPaymentRepository repository,
            IEventBus eventBus,
            IUserInfoProvider userInfoProvider
            )
        {
            _repository = repository;
            _eventBus = eventBus;
            _userInfoProvider = userInfoProvider;
        }

        [Permission(Permissions.Activate, Module = Modules.CurrencyManager)]
        public void ActivateCurrency(string code, string remarks)
        {
            UpdateCurrencyStatus(code, CurrencyStatus.Active, remarks);
        }

        [Permission(Permissions.Deactivate, Module = Modules.CurrencyManager)]
        public void DeactivateCurrency(string code, string remarks)
        {
            UpdateCurrencyStatus(code, CurrencyStatus.Inactive, remarks);
        }

        private void UpdateCurrencyStatus(string code, CurrencyStatus status, string remarks)
        {
            var currency = _repository.Currencies.First(x => x.Code == code);

            if (currency.Status == status)
                return;

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var user = _userInfoProvider.User.Username;
                currency.Status = status;
                currency.UpdatedBy = user;
                currency.DateUpdated = DateTimeOffset.UtcNow;

                if (status == CurrencyStatus.Active)
                {
                    currency.ActivatedBy = user;
                    currency.DateActivated = currency.DateUpdated;
                }
                else
                {
                    currency.DeactivatedBy = user;
                    currency.DateDeactivated = currency.DateUpdated;
                }

                currency.Remarks = remarks;

                _repository.SaveChanges();
                
                var currencyStatusChanged = new CurrencyStatusChanged()
                {
                    Code = currency.Code,
                    Status = currency.Status,
                    
                    DateStatusChanged = currency.Status == CurrencyStatus.Active
                        ? currency.DateActivated.Value
                        : currency.DateDeactivated.Value,
                    StatusChangedBy = currency.Status == CurrencyStatus.Active
                        ? currency.ActivatedBy
                        : currency.DeactivatedBy,
                    
                    Remarks = remarks
                };
                
                _eventBus.Publish(currencyStatusChanged);

                scope.Complete();
            }
        }

    }
}
