using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Payment;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Payment.Events;
using AFT.RegoV2.Core.Player.ApplicationServices.EventHandlers.ActivityLog;
using AFT.RegoV2.Core.Player.Events;
using AFT.RegoV2.Core.Report.ApplicationServices.EventHandlers.PlayerActivityLog;
using AFT.RegoV2.Domain.BoundedContexts.Payment.ApplicationServices.Events;
using AFT.RegoV2.Domain.Payment.Events;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.WinService.Workers
{
    public class PlayerActivityLogWorker : WorkerBase
    {
        private readonly PlayerActivityLogEventHandlers _playerHandlers;
        private readonly DepositActivityLogEventHandlers _depositHandlers;
        private readonly WithdrawActivityLogEventHandlers _withdrawHandlers;
        private readonly PlayerRecordEditActivityLogEventHandlers _playerRecordEditHandlers;
        private readonly PlayerBankAccountLogEventHandlers _playerBankAccountHandlers;

        public PlayerActivityLogWorker(
            IUnityContainer container,
            PlayerActivityLogEventHandlers playerHandlers,
            DepositActivityLogEventHandlers depositHandlers,
            WithdrawActivityLogEventHandlers withdrawHandlers,
            PlayerRecordEditActivityLogEventHandlers playerRecordEditHandlers,
            PlayerBankAccountLogEventHandlers playerBankAccountHandlers
            )
            : base(container)
        {
            _playerHandlers = playerHandlers;
            _depositHandlers = depositHandlers;
            _withdrawHandlers = withdrawHandlers;
            _playerRecordEditHandlers = playerRecordEditHandlers;
            _playerBankAccountHandlers = playerBankAccountHandlers;
        }

        protected override void RegisterEventHandlers()
        {
            // Player category
            RegisterEventHandler<PlayerStatusChanged>(_playerHandlers.Handle);
            RegisterEventHandler<ActivationLinkSent>(_playerHandlers.Handle);
            RegisterEventHandler<PlayerRegistered>((@event) =>
            {
                _playerHandlers.Handle(@event);
                _playerRecordEditHandlers.Handle(@event);
            });
            RegisterEventHandler<PlayerContactVerified>(_playerHandlers.Handle);

            // Deposit category
            RegisterEventHandler<DepositSubmitted>(_depositHandlers.Handle);
            RegisterEventHandler<DepositConfirmed>(_depositHandlers.Handle);
            RegisterEventHandler<DepositVerified>(_depositHandlers.Handle);
            RegisterEventHandler<DepositApproved>(_depositHandlers.Handle);
            RegisterEventHandler<DepositUnverified>(_depositHandlers.Handle);
            
            // Withdraw category
            RegisterEventHandler<WithdrawalCreated>(_withdrawHandlers.Handle);
            RegisterEventHandler<WithdrawalVerified>(_withdrawHandlers.Handle);
            RegisterEventHandler<WithdrawalWagerChecked>(_withdrawHandlers.Handle);
            RegisterEventHandler<WithdrawalInvestigated>(_withdrawHandlers.Handle);
            RegisterEventHandler<WithdrawalAccepted>(_withdrawHandlers.Handle);
            RegisterEventHandler<WithdrawalApproved>(_withdrawHandlers.Handle);
            RegisterEventHandler<WithdrawalCancelled>(_withdrawHandlers.Handle);

            // Player Record Edit category
            RegisterEventHandler<PlayerUpdated>(_playerRecordEditHandlers.Handle);

            // Player Bank Account categry
            RegisterEventHandler<PlayerBankAccountVerified>(_playerBankAccountHandlers.Handle);
            RegisterEventHandler<PlayerBankAccountRejected>(_playerBankAccountHandlers.Handle);

            // Player Transfer Fund categry
            RegisterEventHandler<TransferFundCreated>(_playerHandlers.Handle);
        }
    }
}
