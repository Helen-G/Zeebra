using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Fraud.Validations;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class RiskLevelCommands : MarshalByRefObject, IRiskLevelCommands
    {
        private readonly IFraudRepository _repository;
        private readonly IEventBus _eventBus;
        private readonly ISecurityProvider _securityProvider;

        public RiskLevelCommands(IEventBus serviceBus, IFraudRepository repository, ISecurityProvider securityProvider)
        {
            this._eventBus = serviceBus;
            this._repository = repository;
            _securityProvider = securityProvider;
        }


        private void UpdateStatus(Guid id, bool isActive, string remarks)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var riskLevel = _repository.RiskLevels.SingleOrDefault(x => x.Id == id);

                if (riskLevel == null)
                    throw new ArgumentException("app:fraud.manager.message.invalidRiskLevelId");

                riskLevel.Status = isActive ? Status.Active : Status.Inactive;
                riskLevel.Description = remarks;

                _repository.SaveChanges();

                _eventBus.Publish(new RiskLevelStatusUpdated(id, riskLevel.Status));

                scope.Complete();
            }
        }

        [Permission(Permissions.Activate, Modules.FraudManager)]
        public void Activate(RiskLevelId id, string remarks)
        {
            this.UpdateStatus(id, true, remarks);
        }

        [Permission(Permissions.Deactivate, Modules.FraudManager)]
        public void Deactivate(RiskLevelId id, string remarks)
        {
            this.UpdateStatus(id, false, remarks);
        }

        [Permission(Permissions.Edit, Modules.FraudManager)]
        public void Update(RiskLevel data)
        {
            var validationResult = new UpdateRiskLevelValidator(this._repository)
               .Validate(data);

            if (!validationResult.IsValid)
            {
                throw new RegoValidationException(validationResult);
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var riskLevel = _repository.RiskLevels.Single(x => x.Id == data.Id);

                riskLevel.BrandId = data.BrandId;
                riskLevel.Level = data.Level;
                riskLevel.Name = data.Name;
                riskLevel.Status = data.Status;
                riskLevel.Description = data.Description;
                riskLevel.DateUpdated = DateTimeOffset.Now;
                riskLevel.UpdatedBy = _securityProvider.User.UserName;

                _repository.SaveChanges();

                _eventBus.Publish(new RiskLevelUpdated(data.Id, data.BrandId, data.Level, data.Name, data.Status, data.Description));

                scope.Complete();
            }
        }

        [Permission(Permissions.Add, Modules.FraudManager)]
        public void Create(RiskLevel data)
        {
            if (data.Id == Guid.Empty)
            {
                data.Id = Guid.NewGuid();
                data.Status = Status.Inactive;
            }

            var validationResult = new CreateRiskLevelValidator(this._repository)
                .Validate(data);

            if (!validationResult.IsValid)
            {
                throw new RegoValidationException(validationResult);
            }

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                data.CreatedBy = _securityProvider.User.UserName;
                data.DateCreated = DateTimeOffset.Now;

                _repository.RiskLevels.Add(data);
                _repository.SaveChanges();

                _eventBus.Publish(new RiskLevelCreated(data.Id, data.BrandId, data.Level, data.Name, data.Status, data.Description));

                scope.Complete();
            }
        }

        [Permission(Permissions.Edit, Modules.FraudManager)]
        public void Tag(PlayerId playerId, RiskLevelId riskLevel, string description)
        {
            //TODO: validate

            Guid id = Guid.NewGuid();

            var domain = new Entities.RiskLevel();
            domain.TagPlayer(id, playerId, riskLevel, description);

            domain.Events.ForEach(ev => this._eventBus.Publish(ev));
        }

        [Permission(Permissions.Edit, Modules.FraudManager)]
        public void Untag(PlayerId id, string description)
        {
            //TODO: validate

            var domain = new Entities.RiskLevel();

            var playerRiskLevel =_repository.PlayerRiskLevels.FirstOrDefault(x => x.Id == id);
            if (playerRiskLevel == null)
                throw new ArgumentException("app:fraud.manager.message.invalidPlayerRiskLevelId");

            domain.UntagPlayer(id, playerRiskLevel.PlayerId, playerRiskLevel.RiskLevelId, description);

            domain.Events.ForEach(ev => this._eventBus.Publish(ev));
        }
    }
}
