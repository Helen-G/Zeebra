using AFT.RegoV2.Core.Bonus.EventHandlers;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Brand;
using AFT.RegoV2.Core.Common.Events.Fraud;
using AFT.RegoV2.Core.Common.Events.Game;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Common.Events.Wallet;
using AFT.RegoV2.Domain.Brand.Events;
using AFT.RegoV2.Domain.Payment.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class BonusWorker : WorkerBase
    {
        private readonly PaymentSubscriber _paymentSubscriber;
        private readonly GameSubscriber _gameSubscriber;
        private readonly BrandSubscriber _brandSubscriber;
        private readonly PlayerSubscriber _playerSubscriber;
        private readonly FraudSubscriber _fraudSubscriber;

        public BonusWorker(
            IUnityContainer container, 
            PaymentSubscriber paymentSubscriber,
            GameSubscriber gameSubscriber,
            BrandSubscriber brandSubscriber,
            PlayerSubscriber playerSubscriber,
            FraudSubscriber fraudSubscriber): base(container)
        {
            _paymentSubscriber = paymentSubscriber;
            _gameSubscriber = gameSubscriber;
            _brandSubscriber = brandSubscriber;
            _playerSubscriber = playerSubscriber;
            _fraudSubscriber = fraudSubscriber;
        }

        protected override void RegisterEventHandlers()
        {
            RegisterEventHandler<DepositUnverified>(_paymentSubscriber.Handle);
            RegisterEventHandler<DepositSubmitted>(_paymentSubscriber.Handle);
            RegisterEventHandler<DepositApproved>(_paymentSubscriber.Handle);
            RegisterEventHandler<TransferFundCreated>(_paymentSubscriber.Handle);

            RegisterEventHandler<PlayerRegistered>(_playerSubscriber.Handle);
            RegisterEventHandler<PlayersReferred>(_playerSubscriber.Handle);
            RegisterEventHandler<PlayerContactVerified>(_playerSubscriber.Handle);

            RegisterEventHandler<BrandRegistered>(_brandSubscriber.Handle);
            RegisterEventHandler<BrandUpdated>(_brandSubscriber.Handle);
            RegisterEventHandler<BrandCurrenciesAssigned>(_brandSubscriber.Handle);
            RegisterEventHandler<VipLevelRegistered>(_brandSubscriber.Handle);
            RegisterEventHandler<WalletTemplateCreated>(_brandSubscriber.Handle);
            RegisterEventHandler<WalletTemplateUpdated>(_brandSubscriber.Handle);

            RegisterEventHandler<RiskLevelStatusUpdated>(_fraudSubscriber.Handle);
            RegisterEventHandler<RiskLevelCreated>(_fraudSubscriber.Handle);
            RegisterEventHandler<RiskLevelTagPlayer>(_fraudSubscriber.Handle);
            RegisterEventHandler<RiskLevelUntagPlayer>(_fraudSubscriber.Handle);

            RegisterEventHandler<GameCreated>(_gameSubscriber.Handle);
            RegisterEventHandler<GameUpdated>(_gameSubscriber.Handle);
            RegisterEventHandler<GameDeleted>(_gameSubscriber.Handle);
            RegisterEventHandler<TransactionProcessed>(_gameSubscriber.Handle);
        }
    }
}