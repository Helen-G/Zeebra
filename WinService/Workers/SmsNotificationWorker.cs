using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.Core.Common.Commands;
using AFT.RegoV2.Core.Common.Interfaces;
using WinService.Workers.Common;

namespace WinService.Workers
{
    public class SmsNotificationWorker : NotificationWorkerBase<SmsCommandMessage>
    {
        private readonly ISmsNotifier _smsNotifier;

        public SmsNotificationWorker(
            ISmsNotifier smsNotifier,
            IEventRepository eventRepository,
            IServiceBus serviceBus)
            : base(eventRepository, serviceBus)
        {
            _smsNotifier = smsNotifier;
        }

        public override void ProcessMessage(SmsCommandMessage message)
        {
            _smsNotifier.SendSms(
                message.PhoneNumber,
                message.Body,
                FinalizeMessage);
        }
    }
}