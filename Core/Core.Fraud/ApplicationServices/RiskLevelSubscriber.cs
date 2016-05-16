using System.Linq;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Domain.Brand.Events;
using LocalBrand = AFT.RegoV2.Core.Fraud.Data.Brand;

namespace AFT.RegoV2.Core.Fraud.ApplicationServices
{
    public class RiskLevelSubscriber : IBusSubscriber,
        IConsumes<BrandRegistered>,
        IConsumes<BrandUpdated>,
        IConsumes<RiskLevelTagPlayer>,
        IConsumes<RiskLevelUntagPlayer>
    {
        private readonly IFraudRepository _repository;

        public RiskLevelSubscriber(IFraudRepository repository)
        {
            this._repository = repository;
        }

        public void Consume(BrandRegistered @event)
        {
            if (!this._repository.Brands.Any(x => x.Id == @event.Id))
            {
                this._repository.Brands.Add(new LocalBrand
                {
                    Id = @event.Id,
                    Name = @event.Name,
                    Code = @event.Code,
                    LicenseeId = @event.LicenseeId,
                    LicenseeName = @event.LicenseeName,
                    TimeZoneId = @event.TimeZoneId
                });

                this._repository.SaveChanges();
            }
        }

        public void Consume(BrandUpdated @event)
        {
            var brand = this._repository.Brands.Single(x => x.Id == @event.Id);

            brand.Code = @event.Code;
            brand.Name = @event.Name;
            brand.LicenseeId = @event.LicenseeId;
            brand.LicenseeName = @event.LicenseeName;
            brand.TimeZoneId = @event.TimeZoneId;

            _repository.SaveChanges();
        }

        public void Consume(RiskLevelTagPlayer @event)
        {
            if (!this._repository.PlayerRiskLevels.Any(x => x.Id == @event.Id))
            {
                PlayerRiskLevel entity = new PlayerRiskLevel();
                entity.Id = @event.Id;
                entity.PlayerId = @event.PlayerId;
                entity.RiskLevelId = @event.RiskLevelId;
                entity.Description = @event.Description;

                entity.CreatedBy = @event.EventCreatedBy;
                entity.DateCreated = @event.EventCreated;

                this._repository.PlayerRiskLevels.Add(entity);
                this._repository.SaveChanges();
            }
        }

        public void Consume(RiskLevelUntagPlayer @event)
        {
            var entity = this._repository.PlayerRiskLevels.SingleOrDefault(x => x.Id == @event.Id);
            if (entity != null)
            {
                this._repository.PlayerRiskLevels.Remove(entity);
                this._repository.SaveChanges();
            }
        }
    }
}
