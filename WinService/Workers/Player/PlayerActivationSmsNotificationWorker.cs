using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Events;
using WinService.Workers.Common;

namespace WinService.Workers.Player
{
    public class PlayerActivationSmsNotificationWorker : NotificationWorkerBase<PlayerActivationSmsCommandMessage>
    {
        private readonly ISmsNotifier _smsNotifier;

        public PlayerActivationSmsNotificationWorker(
            ISmsNotifier smsNotifier,
            IEventRepository eventRepository,
            IServiceBus serviceBus)
            : base(eventRepository, serviceBus)
        {
            _smsNotifier = smsNotifier;
        }

        public override void ProcessMessage(PlayerActivationSmsCommandMessage message)
        {
            _smsNotifier.SendSms(message.PhoneNumber, message.Body, FinalizeMessage);

            _serviceBus.PublishMessage(new ActivationLinkSent(message.PlayerId, ContactType.Mobile, message.Token));
        }
    }
}