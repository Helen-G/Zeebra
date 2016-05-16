using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Brand.Events;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class GameSubscriber : IBusSubscriber,
        IConsumes<PlayerRegistered>,
        IConsumes<BrandRegistered>,
        IConsumes<BrandUpdated>,
        IConsumes<PlayerUpdated>,
        IConsumes<LicenseeCreated>,
        IConsumes<BrandProductsAssigned>,
        IConsumes<VipLevelRegistered>,
        IConsumes<VipLevelUpdated>,
        IConsumes<CurrencyCreated>,
        IConsumes<CurrencyUpdated>,
        IConsumes<LanguageCreated>,
        IConsumes<LanguageUpdated>
    {
        private readonly IGameRepository _repository;

        public GameSubscriber(IGameRepository repository)
        {
            _repository = repository;
        }

        public void Consume(PlayerRegistered @event)
        {
            var gamePlayer = new Player
            {
                Id = @event.PlayerId,
                Name = @event.DisplayName,
                BrandId = @event.BrandId,
                CultureCode = @event.CultureCode,
                CurrencyCode = @event.CurrencyCode,
                VipLevelId = @event.VipLevelId
            };
            _repository.Players.Add(gamePlayer);
            _repository.SaveChanges();
        }

        public void Consume(LicenseeCreated @event)
        {
            _repository.Licensees.Add(new Licensee {Id = @event.Id});
            _repository.SaveChanges();
        }

        public void Consume(BrandRegistered @event)
        {
            var brand = new Data.Brand
            {
                Id = @event.Id,
                Code = @event.Code,
                LicenseeId = @event.LicenseeId,
                TimezoneId = @event.TimeZoneId
            };

            _repository.Brands.Add(brand);

            _repository.SaveChanges();
        }

        public void Consume(BrandUpdated @event)
        {
            var brand = _repository.Brands.Single(x => x.Id == @event.Id);

            brand.Code = @event.Code;
            brand.TimezoneId = @event.TimeZoneId;

            _repository.SaveChanges();
        }

        public void Consume(PlayerUpdated @event)
        {
            var player = _repository.Players.Single(x => x.Id == @event.Player.Id);

            player.VipLevelId = @event.Player.VipLevelId;

            _repository.SaveChanges();
        }

        public void Consume(BrandProductsAssigned @event)
        {
            var brand = _repository.Brands.Where(x => @event.BrandId == x.Id).Include(x => x.BrandGameProviderConfigurations).Single();
            
            brand.BrandGameProviderConfigurations = new Collection<BrandGameProviderConfiguration>();
            
            var gameProviders = _repository.GameProviders
                .Include(x => x.GameProviderConfigurations)
                .Where(x => @event.ProductsIds.Contains(x.Id)).ToArray();

            if (gameProviders.Length == 0)
                throw new RegoException("Unable to assign product. The Product does not exist in the Game domain.");
            gameProviders.ForEach(x => 
                brand.BrandGameProviderConfigurations.Add(new BrandGameProviderConfiguration
                    {
                        Id = Guid.NewGuid(),
                        BrandId = brand.Id,
                        GameProviderConfigurationId = x.GameProviderConfigurations.First().Id,
                        GameProviderId = x.Id
                    })
            );

            _repository.SaveChanges();
        }

        public void Consume(VipLevelRegistered @event)
        {
            _repository.VipLevels.Add(new VipLevel 
            {
                Id = @event.Id,
                BrandId = @event.BrandId,
                VipLevelLimits = @event.VipLevelLimits.Select(x => new VipLevelGameProviderBetLimit
                {
                    VipLevelId = x.VipLevelId,
                    BetLimitId = x.BetLimitId,
                    GameProviderId = x.GameProviderId,
                    CurrencyCode = x.CurrencyCode
                }).ToList()
            });
            _repository.SaveChanges();
        }

        public void Consume(VipLevelUpdated @event)
        {
            var vipLevel = _repository.VipLevels.Single(x => x.Id == @event.Id);
            vipLevel.VipLevelLimits.Clear();
            vipLevel.VipLevelLimits = @event.VipLevelLimits.Select(x => new VipLevelGameProviderBetLimit
            {
                VipLevelId = x.VipLevelId,
                BetLimitId = x.BetLimitId,
                GameProviderId = x.GameProviderId,
                CurrencyCode = x.CurrencyCode
            }).ToList();
            _repository.SaveChanges();
        }

        public void Consume(LanguageCreated @event)
        {
            var culture = new GameCulture {Code = @event.Code};
            _repository.Cultures.Add(culture);
            _repository.SaveChanges();
        }

        public void Consume(LanguageUpdated @event)
        {
            var cultures = _repository.Cultures.Single(x => x.Code == @event.Code);
            cultures.Code = @event.Code;
            _repository.SaveChanges();
        }

        public void Consume(CurrencyCreated @event)
        {
            var currency = new GameCurrency { Code = @event.Code };
            _repository.Currencies.Add(currency);
            _repository.SaveChanges();
        }

        public void Consume(CurrencyUpdated @event)
        {
            var currencies = _repository.Currencies.Single(x => x.Code == @event.Code);
            currencies.Code = @event.Code;
            _repository.SaveChanges();
        }

    }
}
