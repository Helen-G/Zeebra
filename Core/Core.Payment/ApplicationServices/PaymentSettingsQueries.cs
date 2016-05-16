using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment.ApplicationServices;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;

namespace AFT.RegoV2.Core.Payment.ApplicationServices
{
    public class PaymentSettingsQueries : MarshalByRefObject, IApplicationService
    {
        private readonly IPaymentQueries _paymentQueries;
        private readonly IPlayerQueries _playerQueries;
        private readonly PaymentSettingsCommands _settingsCommands;

        public PaymentSettingsQueries(IPaymentQueries paymentQueries, IPlayerQueries playerQueries, PaymentSettingsCommands settingsCommands)
        {
            _paymentQueries = paymentQueries;
            _playerQueries = playerQueries;
            _settingsCommands = settingsCommands;
        }

        public object GetBankAccounts(Guid brandId, string currencyCode)
        {
            return _paymentQueries.GetBankAccounts(brandId, currencyCode)
                .Select(x => new
                {
                    x.Id,
                    Name = string.Format("Offline - {0}", x.AccountId)
                });
        }

        public object GetVipLevels(Guid? brandId)
        {
            return brandId.HasValue
                ? _playerQueries.VipLevels.Where(x => x.BrandId == brandId).Select(x => new { x.Id, x.Name })
                : _playerQueries.VipLevels.Select(x => new { x.Id, x.Name });
        }

        [Permission(Permissions.Add, Module = Modules.PaymentSettings)]
        [Permission(Permissions.Edit, Module = Modules.PaymentSettings)]
        public PaymentSettingSaveResult SaveSetting(SavePaymentSettingsCommand model)
        {
            string message;
            var paymentSettingsId = model.Id;

            if (model.Id == Guid.Empty)
            {
                paymentSettingsId = _settingsCommands.AddSettings(model);
                message = "CreatedSuccessfully";
            }
            else
            {
                _settingsCommands.UpdateSettings(model);
                message = "UpdatedSuccessfully";
            }

            return new PaymentSettingSaveResult
            {
                Message = message,
                PaymentSettingsId = paymentSettingsId
            };
        }
    }

    public class PaymentSettingSaveResult
    {
        public string Message { get; set; }
        public Guid PaymentSettingsId { get; set; }
    }
}
