using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Events;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Player.ApplicationServices
{
    public class IdentificationDocumentSettingsService : MarshalByRefObject, IApplicationService
    {
        private readonly IPlayerRepository _repository;
        private readonly ISecurityProvider _security;
        private readonly IEventBus _eventBus;

        public IdentificationDocumentSettingsService(
            IPlayerRepository repository,
            ISecurityProvider security,
            IEventBus eventBus)
        {
            _repository = repository;
            _security = security;
            _eventBus = eventBus;
        }

        #region Queries
        public IdentificationDocumentSettings GetSettingById(Guid settingId)
        {
            var setting = _repository.IdentificationDocumentSettings
                .Single(o => o.Id == settingId);

            return setting;
        }

        [Permission(Permissions.View, Module = Modules.IdentificationDocumentSettings)]
        public IQueryable<IdentificationDocumentSettings> GetSettings()
        {
            return _repository.IdentificationDocumentSettings
                .Include(o => o.Brand)
                .AsQueryable();
        }

        #endregion

        [Permission(Permissions.Add, Module = Modules.IdentificationDocumentSettings)]
        public IdentificationDocumentSettings CreateSetting(IdentificationDocumentSettings setting)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                setting.Id = Guid.NewGuid();
                setting.CreatedBy = _security.User.UserName;
                setting.CreatedOn = DateTimeOffset.UtcNow;

                _repository.IdentificationDocumentSettings.Add(setting);

                _repository.SaveChanges();

                _eventBus.Publish(new IdentificationDocumentSettingsCreated(setting));

                scope.Complete();
            }

            return setting;
        }

        [Permission(Permissions.Edit, Module = Modules.IdentificationDocumentSettings)]
        public IdentificationDocumentSettings UpdateSetting(IdentificationDocumentSettings data)
        {
            var setting = GetSettingById(data.Id);
            if (setting == null)
                throw new RegoException("Setting not found");

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                setting.BrandId = data.BrandId;
                setting.LicenseeId = data.LicenseeId;
                setting.PaymentMethod = data.PaymentMethod;
                setting.TransactionType = data.TransactionType;
                setting.IdBack = data.IdBack;
                setting.IdFront = data.IdFront;
                setting.CreditCardBack = data.CreditCardBack;
                setting.CreditCardFront = data.CreditCardFront;
                setting.DCF = data.DCF;
                setting.POA = data.POA;
                setting.Remark = data.Remark;

                setting.UpdatedBy = _security.User.UserName;
                setting.UpdatedOn = DateTimeOffset.UtcNow;

                _repository.SaveChanges();

                _eventBus.Publish(new IdentificationDocumentSettingsUpdated(setting));

                scope.Complete();

                return setting;
            }
        }
    }
}
