using System;
using System.Data.Entity.Migrations;
using System.Linq;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data.Payment;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Events.Games;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Shared;


namespace AFT.RegoV2.ApplicationServices.Payment
{
    public class PaymentSubscriber : IBusSubscriber,
        IConsumes<PlayerVipLevelChanged>,
        IConsumes<PlayerRegistered>,
        IConsumes<BrandRegistered>,
        IConsumes<PlayerUpdated>,
        IConsumes<BetPlaced>,
        IConsumes<VipLevelUpdated>,
        IConsumes<VipLevelRegistered>,
        IConsumes<BrandCurrenciesAssigned>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly BrandQueries _brandQueries;

        public PaymentSubscriber(IPaymentRepository playerRepository,
            BrandQueries brandQueries)
        {
            _paymentRepository = playerRepository;
            _brandQueries = brandQueries;
        }

        public void Consume(PlayerVipLevelChanged @event)
        {
            var player = _paymentRepository.Players.FirstOrDefault(x => x.Id == @event.PlayerId);
            if (player == null)
                return;

            player.VipLevelId = @event.VipLevelId;
            _paymentRepository.SaveChanges();
        }

        public void Consume(PlayerRegistered @event)
        {
            var player = @event;

            var defaultPaymentLevelId = _brandQueries.GetDefaultPaymentLevelId(player.BrandId, player.CurrencyCode);

            var paymentLevel = player.PaymentLevelId.HasValue
                ? _paymentRepository.PaymentLevels.SingleOrDefault(l => l.Id == player.PaymentLevelId.Value)
                : _paymentRepository.PaymentLevels.SingleOrDefault(l => l.Id == defaultPaymentLevelId);

            if (paymentLevel == null)
            {
                throw new RegoException("No appropriate payment level found. Brand: " + player.BrandId + " Currency: " +
                                    player.CurrencyCode);
            }
            _paymentRepository.PlayerPaymentLevels.Add(new PlayerPaymentLevel
            {
                PlayerId = player.PlayerId,
                PaymentLevel = paymentLevel
            });

            //            var newPlayer = new Domain.Payment.Data.Player
            //            {
            //                Id = player.Id,
            //                BrandId = player.BrandId,
            //                CurrencyCode = player.CurrencyCode,
            //                PhoneNumber = player.PhoneNumber,
            //                Username = player.UserName,
            //                VipLevelId = player.VipLevelId,
            //                Email = player.Email,
            //                DateRegistered = player.DateRegistered,
            //            };
            //
            //            _paymentRepository.Players.Add(newPlayer);
            _paymentRepository.SaveChanges();
        }

        public void Consume(BrandRegistered @event)
        {
            var brand = _paymentRepository.Brands.SingleOrDefault(x => x.Id == @event.Id);
            if (brand == null)
            {
                _paymentRepository.Brands.Add(new Brand
                {
                    Id = @event.Id,
                    Name = @event.Name,
                    Code = @event.Code,
                    LicenseeId = @event.LicenseeId,
                    LicenseeName = @event.LicenseeName
                });
                _paymentRepository.SaveChanges();
            }
        }

        public void Consume(PlayerUpdated @event)
        {
            var playerPaymentLevel =
                _paymentRepository.PlayerPaymentLevels.FirstOrDefault(p => p.PlayerId == @event.Player.Id);

            if (playerPaymentLevel != null)
            {
                var paymentLevel = _paymentRepository.PaymentLevels.Single(p => p.Id == @event.Player.PaymentLevelId);
                playerPaymentLevel.PaymentLevel = paymentLevel;
                _paymentRepository.SaveChanges();
            }
        }

        public void Consume(BetPlaced @event)
        {
            var playerId = @event.PlayerId;
            var betAmount = @event.Amount;
            var offlineDeposits =
                _paymentRepository
                    .OfflineDeposits
                    .Where(
                        x =>
                            x.Player.Id == playerId && x.DepositWagering != 0 &&
                            x.Status == OfflineDepositStatus.Approved)
                    .OrderBy(x => x.Created)
                    .ToArray();
            if (!offlineDeposits.Any())
                return;

            var count = 0;
            while (betAmount != 0 && count < offlineDeposits.Length)
            {
                var deposit = offlineDeposits[count];
                deposit.DepositWagering = deposit.DepositWagering - betAmount;

                if (deposit.DepositWagering >= 0)
                    betAmount = 0;
                else
                    betAmount = -1 * deposit.DepositWagering;

                if (deposit.DepositWagering < 0)
                    deposit.DepositWagering = 0;

                count++;
            }
            _paymentRepository.SaveChanges();
        }

        public void Consume(VipLevelUpdated @event)
        {
            var vipLevel = _paymentRepository.VipLevels.FirstOrDefault(x => x.Id == @event.Id);
            
            if (vipLevel == null)
                throw new RegoException("No appropriate Vip Level found. Brand: " + @event.BrandId + " Code: " + @event.Code);

            vipLevel.BrandId = @event.BrandId;
            vipLevel.Name = @event.Name;

            _paymentRepository.SaveChanges();
        }

        public void Consume(VipLevelRegistered @event)
        {
            _paymentRepository.VipLevels.Add(new VipLevel
            {
                Id = @event.Id,
                Name = @event.Name,
                BrandId = @event.BrandId
            });

            _paymentRepository.SaveChanges();
        }

        public void Consume(BrandCurrenciesAssigned message)
        {
            //reassign Base currency
            var paymentBrand = _paymentRepository.Brands.Single(x => x.Id == message.BrandId);
            var oldBaseCurrencyCode = paymentBrand.BaseCurrencyCode;

            if (oldBaseCurrencyCode != message.BaseCurrency)
            {
                //remove old Currency Exchanges
                var oldCurrencyExchanges = _paymentRepository.CurrencyExchanges
                    .Where(x => x.BrandId == message.BrandId)
                    .ToArray();

                foreach (var oldCurrencyExchange in oldCurrencyExchanges)
                {
                    _paymentRepository.CurrencyExchanges.Remove(oldCurrencyExchange);
                }

                _paymentRepository.SaveChanges();

                //remove old Currencies
                var oldCurrencies = _paymentRepository.BrandCurrencies
                    .Where(x => x.BrandId == message.BrandId)
                    .ToArray();

                foreach (var oldCurrency in oldCurrencies)
                {
                    _paymentRepository.BrandCurrencies.Remove(oldCurrency);
                }

                _paymentRepository.SaveChanges();
                
                //add new currencies
                foreach (var newCurrency in message.Currencies)
                {
                    var brandCurrency = new BrandCurrency { BrandId = message.BrandId, CurrencyCode = newCurrency };

                    _paymentRepository.BrandCurrencies.AddOrUpdate(brandCurrency);
                }

                _paymentRepository.SaveChanges();
                
                //Set Base Currency
                paymentBrand.BaseCurrencyCode = message.BaseCurrency;
                _paymentRepository.SaveChanges();

                //add new base currency exchange
                var baseCurrencyExchange = new CurrencyExchange
                {
                    BrandId = message.BrandId,
                    CurrencyToCode = message.BaseCurrency,
                    IsBaseCurrency = true,
                    CurrentRate = 1,
                    //todo: need to update
                    CreatedBy = "System",
                    DateCreated = DateTimeOffset.UtcNow
                };

                _paymentRepository.CurrencyExchanges.AddOrUpdate(baseCurrencyExchange);
                _paymentRepository.SaveChanges();

                return;
            }

            //remove not exist Currency Exchanges
            var notExistCurrencyExchanges = _paymentRepository.CurrencyExchanges
                    .Where(x => x.BrandId == message.BrandId &&
                        !message.Currencies.Contains(x.CurrencyToCode))
                    .ToArray();

            foreach (var oldCurrencyExchange in notExistCurrencyExchanges)
            {
                _paymentRepository.CurrencyExchanges.Remove(oldCurrencyExchange);
            }

            _paymentRepository.SaveChanges();

            //remove not exist Currencies
            var notExistCurrencies = _paymentRepository.BrandCurrencies
                .Where(x => x.BrandId == message.BrandId &&
                    !message.Currencies.Contains(x.CurrencyCode))
                .ToArray();

            foreach (var oldCurrency in notExistCurrencies)
            {
                _paymentRepository.BrandCurrencies.Remove(oldCurrency);
            }

            _paymentRepository.SaveChanges();

            //add new currencies
            var existingCurrencies = _paymentRepository.BrandCurrencies
                .Where(x => x.BrandId == message.BrandId).Select(c => c.CurrencyCode).ToArray();

            var newCurrencies = 
                message.Currencies.Where(c => 
                    !existingCurrencies.Contains(c)).ToArray();

            foreach (var newCurrency in newCurrencies)
            {
                var brandCurrency = new BrandCurrency {BrandId = message.BrandId, CurrencyCode = newCurrency};

                _paymentRepository.BrandCurrencies.AddOrUpdate(brandCurrency);
            }

            _paymentRepository.SaveChanges();
        }
    }
}