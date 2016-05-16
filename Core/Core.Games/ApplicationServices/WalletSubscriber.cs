using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Game.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Domain.BoundedContexts.Payment.ApplicationServices.Events;
using ServiceStack.Common.Extensions;

namespace AFT.RegoV2.Core.Game.ApplicationServices
{
    public class WalletSubscriber : IBusSubscriber,
        IConsumes<PlayerRegistered>,
        IConsumes<WalletTemplateCreated>,
        IConsumes<WalletTemplateUpdated>,
        IConsumes<WithdrawalCreated>,
        IConsumes<WithdrawalCancelled>
    {
        private readonly IGameRepository _walletRepository;
        private readonly IWalletCommands _walletCommands;

        public WalletSubscriber(IGameRepository walletRepository, IWalletCommands walletCommands)
        {
            _walletRepository = walletRepository;
            _walletCommands = walletCommands;
        }

        public void Consume(PlayerRegistered @event)
        {
            var brand = _walletRepository.Brands.Single(b => b.Id == @event.BrandId);
            var walletTemplates = _walletRepository.WalletTemplates.Where(t => t.BrandId == @event.BrandId);

            CreateWalletsForPlayer(@event.PlayerId, brand, walletTemplates);

            _walletRepository.SaveChanges();
        }

        public void Consume(WalletTemplateCreated @event)
        {
            CreateWalletTemplates(@event.BrandId, @event.WalletTemplates);
        }

        public void Consume(WalletTemplateUpdated @event)
        {
            //create new wallets if new templates are being added
            CreateWalletTemplates(@event.BrandId, @event.NewWalletTemplates);
            CreateWalletsForPlayers(@event.BrandId, @event.NewWalletTemplates.Select(wt => wt.Id).ToArray());

            //update existing templates
            var remainTemplateIds = @event.RemainedWalletTemplates.Select(x => x.Id).ToArray();
            var walletTemplatesToUpdate = _walletRepository.WalletTemplates.Where(x => remainTemplateIds.Contains(x.Id)).ToArray();
            foreach (var remainedWalletTemplate in @event.RemainedWalletTemplates)
            {
                var templateToUpdate = walletTemplatesToUpdate.Single(t => t.Id == remainedWalletTemplate.Id);
                templateToUpdate.Name = remainedWalletTemplate.Name;
                templateToUpdate.WalletTemplateGameProviders = remainedWalletTemplate
                    .ProductIds
                    .Select(productId => new WalletTemplateGameProvider
                    {
                        Id = Guid.NewGuid(),
                        GameProviderId = productId,
                        WalletTemplateId = remainedWalletTemplate.Id
                    }).ToList();
            }

            //archive templates which were removed
            var templatesToArchive = _walletRepository.WalletTemplates.Where(x => @event.RemovedWalletTemplateIds.Contains(x.Id));
            foreach (var wallet in templatesToArchive)
                wallet.IsArchived = true;

            _walletRepository.SaveChanges();
        }

        public void Consume(WithdrawalCreated @event)
        {
                    var playerId = @event.PlayerId;
            _walletCommands.Lock(playerId, new LockUnlockParams { Amount = @event.Amount, Type = LockType.Withdrawal });
        }

        public void Consume(WithdrawalCancelled @event)
        {
            var playerId = @event.PlayerId;
            _walletCommands.Unlock(playerId, new LockUnlockParams { Amount = @event.Amount, Type = LockType.Withdrawal, Description = @event.Remarks});
        }

        private void CreateWalletsForPlayers(Guid brandId, Guid[] templatesIds)
        {
            var brand = _walletRepository.Brands.Single(b => b.Id == brandId);
            var playerIDs = _walletRepository.Wallets.Where(w => w.Brand.Id == brandId).Select(w => w.PlayerId).Distinct();
            var templates = _walletRepository.WalletTemplates.Where(t => t.BrandId == brandId && templatesIds.Contains(t.Id));
            foreach (var playerId in playerIDs)
            {
                CreateWalletsForPlayer(playerId, brand, templates);
            }

            _walletRepository.SaveChanges();
        }



        private void CreateWalletTemplates(Guid brandId, WalletTemplateDto[] walletTemplates)
        {
            foreach (var walletTemplate in walletTemplates)
            {
                var template = new WalletTemplate
                {
                    Id = walletTemplate.Id,
                    Name = walletTemplate.Name,
                    BrandId = brandId,
                    CurrencyCode = walletTemplate.CurrencyCode,
                    IsMain = walletTemplate.IsMain,
                    WalletTemplateGameProviders = walletTemplate
                        .ProductIds
                        .Select(productId => new WalletTemplateGameProvider
                        {
                            Id = Guid.NewGuid(),
                            GameProviderId = productId,
                            WalletTemplateId = walletTemplate.Id
                        }).ToList()
                };
                _walletRepository.WalletTemplates.Add(template);
            }

            _walletRepository.SaveChanges();
        }

        private void CreateWalletsForPlayer(Guid playerId, Core.Game.Data.Brand brand, IEnumerable<WalletTemplate> walletTemplates)
        {
            walletTemplates.ForEach(walletTemplate =>
                _walletRepository.Wallets.Add(new Wallet
                {
                    PlayerId = playerId,
                    Brand = brand,
                    Template = walletTemplate
                }));
        }

        

    }
}