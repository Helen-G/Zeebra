using System.Linq;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Core.Bonus.EventHandlers
{
    public class FraudSubscriber
    {
        private readonly IUnityContainer _container;
        private const string NoPlayerFormat = "No player found with Id: {0}";
        private const string NoRiskLevelFormat = "No risk level found with Id: {0}";

        public FraudSubscriber(IUnityContainer container)
        {
            _container = container;
        }

        public void Handle(RiskLevelTagPlayer @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();
            var player = bonusRepository.Players.SingleOrDefault(x => x.Id == @event.PlayerId);
            if (player == null)
                throw new RegoException(string.Format(NoPlayerFormat, @event.PlayerId));

            var riskLevel = player.Brand.RiskLevels.SingleOrDefault(x => x.Id == @event.RiskLevelId);
            if (riskLevel == null)
                throw new RegoException(string.Format(NoRiskLevelFormat, @event.RiskLevelId));

            if (!player.RiskLevels.Exists(rl => rl.Id == @event.RiskLevelId))
            {
                player.RiskLevels.Add(riskLevel);
                bonusRepository.SaveChanges();
            }
        }

        public void Handle(RiskLevelUntagPlayer @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();

            var player = bonusRepository.Players.FirstOrDefault(x => x.Id == @event.PlayerId);
            if (player == null)
                throw new RegoException(string.Format(NoPlayerFormat, @event.PlayerId));

            var riskLevel = player.RiskLevels.FirstOrDefault(rl => rl.Id == @event.RiskLevelId);
            if (riskLevel != null)
                player.RiskLevels.Remove(riskLevel);

            bonusRepository.SaveChanges();
        }

        public void Handle(RiskLevelStatusUpdated @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();
            var riskLevel = bonusRepository.Brands.SelectMany(b => b.RiskLevels).SingleOrDefault(x => x.Id == @event.Id);
            if (riskLevel == null)
                throw new RegoException(string.Format(NoRiskLevelFormat, @event.Id));

            riskLevel.IsActive = @event.NewStatus == Status.Active;
            bonusRepository.SaveChanges();
        }

        public void Handle(RiskLevelCreated @event)
        {
            var bonusRepository = _container.Resolve<IBonusRepository>();

            var brand = bonusRepository.Brands.SingleOrDefault(x => x.Id == @event.BrandId);
            if (brand == null)
                throw new RegoException(string.Format("No brand found with Id: {0}", @event.BrandId));

            var newRiskLevel = new RiskLevel
            {
                Id = @event.Id,
                IsActive = @event.Status == Status.Active
            };

            brand.RiskLevels.Add(newRiskLevel);
            bonusRepository.SaveChanges();
        }
    }
}