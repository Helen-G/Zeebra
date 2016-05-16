using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Brand.ApplicationServices
{
    public class BrandSubscriber : IBusSubscriber,
        IConsumes<RiskLevelCreated>,
        IConsumes<RiskLevelUpdated>,
        IConsumes<RiskLevelStatusUpdated>,
        IConsumes<CurrencyUpdated>,
        IConsumes<CurrencyCreated>
    {
        private readonly IBrandRepository _brandRepository;

        public BrandSubscriber(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public void Consume(RiskLevelCreated message)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var riskLevel = new RiskLevel
                {
                    Id = message.Id,
                    BrandId = message.BrandId,
                    Name = message.Name,
                    Level = message.Level,
                    Description = message.Description,
                    Status = message.Status
                };

                _brandRepository.RiskLevels.Add(riskLevel);
                _brandRepository.SaveChanges();

                scope.Complete();
            }
        }

        public void Consume(RiskLevelUpdated message)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var riskLevel = _brandRepository.RiskLevels.Single(x => x.Id == message.Id);

                riskLevel.BrandId = message.BrandId;
                riskLevel.Name = message.Name;
                riskLevel.Level = message.Level;
                riskLevel.Description = message.Description;
                riskLevel.Status = message.Status;

                _brandRepository.SaveChanges();

                scope.Complete();
            }
        }

        public void Consume(RiskLevelStatusUpdated message)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var riskLevel = _brandRepository.RiskLevels.Single(x => x.Id == message.Id);

                riskLevel.Status = message.NewStatus;

                _brandRepository.SaveChanges();

                scope.Complete();
            }
        }

        public void Consume(CurrencyUpdated message)
        {
            var currency = _brandRepository.Currencies.Single(x => x.Code == message.OldCode);

            if (currency == null)
                throw new RegoException("No appropriate Currency found. Code: " + message.Code);
            
            currency.Code = message.Code;
            currency.Name = message.Name;

            _brandRepository.SaveChanges();
        }

        public void Consume(CurrencyCreated message)
        {
            if (_brandRepository.Currencies.FirstOrDefault(x => x.Code == message.Code) != null)
                throw new RegoException("Currency Code: " + message.Code + "already exist");

            var currency = new Brand.Data.Currency
            {
                Code = message.Code, 
                Name = message.Name
            };

            _brandRepository.Currencies.Add(currency);

            _brandRepository.SaveChanges();
        }
    }
}
