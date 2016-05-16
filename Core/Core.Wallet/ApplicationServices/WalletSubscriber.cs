using System;
using System.Linq;
using AFT.RegoV2.Core.Brand.Events;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Wallet.Data;
using AFT.RegoV2.Core.Wallet.Interfaces;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Domain.BoundedContexts.Payment.ApplicationServices.Events;

namespace AFT.RegoV2.Core.Wallet.ApplicationServices
{
    public class WalletSubscriber : IBusSubscriber,
        IConsumes<PlayerRegistered>,
        IConsumes<WalletStructureCreated>,
        IConsumes<WalletStructureUpdated>,
        IConsumes<WithdrawalCreated>,
        IConsumes<WithdrawalCancelled>
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletCommands _walletCommands;

        public WalletSubscriber(IWalletRepository walletRepository, IWalletCommands walletCommands)
        {
            _walletRepository = walletRepository;
            _walletCommands = walletCommands;
        }

        public void Consume(PlayerRegistered @event)
        {
            var templates = _walletRepository.Templates.Where(t => t.BrandId == @event.BrandId);
            foreach (var walletTemplate in templates)
            {
                var wallet = new Data.Wallet
                {
                    PlayerId = @event.PlayerId,
                    BrandId = @event.BrandId,
                    Template = walletTemplate
                };

                _walletRepository.Wallets.Add(wallet);
            }

            _walletRepository.SaveChanges();
        }

        public void Consume(WalletStructureCreated @event)
        {
            CreateWalletTemplates(@event.WalletTemplates);
        }

        private void CreateWalletTemplates(WalletTemplateDto[] walletTemplates)
        {
            foreach (var walletTemplate in walletTemplates)
            {
                var template = new WalletTemplate
                {
                    Id = walletTemplate.Id,
                    Name = walletTemplate.Name,
                    BrandId = walletTemplate.BrandId,
                    CurrencyCode = walletTemplate.CurrencyCode,
                    IsMain = walletTemplate.IsMain
                };
                _walletRepository.Templates.Add(template);
            }

            _walletRepository.SaveChanges();
        }

        public void Consume(WalletStructureUpdated @event)
        {
            //create new wallets if new templates are being added
            CreateWalletTemplates(@event.NewWalletTemplates);
            CreateWalletsForPlayers(@event.BrandId, @event.NewWalletTemplates.Select(wt => wt.Id).ToArray());

            //update existing templates
            var remainTemplateIds = @event.RemainedWalletTemplates.Select(x => x.Id).ToArray();
            var walletTemplatesToUpdate = _walletRepository.Templates.Where(x => remainTemplateIds.Contains(x.Id)).ToArray();
            foreach (var remainedWalletTemplate in @event.RemainedWalletTemplates)
            {
                var templateToUpdate = walletTemplatesToUpdate.Single(t => t.Id == remainedWalletTemplate.Id);
                templateToUpdate.Name = remainedWalletTemplate.Name;
            }

            //archive templates which were removed
            var templatesToArchive = _walletRepository.Templates.Where(x => @event.RemovedWalletTemplateIds.Contains(x.Id));
            foreach (var wallet in templatesToArchive)
                wallet.IsArchived = true;

            _walletRepository.SaveChanges();
        }

        private void CreateWalletsForPlayers(Guid brandId, Guid[] templatesIds)
        {
            var playerIDs = _walletRepository.Wallets.Where(w => w.BrandId == brandId).Select(w => w.PlayerId).Distinct();
            var templates = _walletRepository.Templates.Where(t => t.BrandId == brandId && templatesIds.Contains(t.Id));
            foreach (var playerId in playerIDs)
            {
                foreach (var walletTemplate in templates)
                {
                    var wallet = new Data.Wallet
                    {
                        PlayerId = playerId,
                        BrandId = brandId,
                        Template = walletTemplate
                    };

                    _walletRepository.Wallets.Add(wallet);
                }
            }

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
    }
}