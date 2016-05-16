using AFT.RegoV2.BoundedContexts.Event;
using AFT.RegoV2.Core.Common.Commands;
using AFT.RegoV2.Core.Common.Interfaces;
using WinService.Workers.Common;

namespace WinService.Workers
{
    public class EmailNotificationWorker : NotificationWorkerBase<EmailCommandMessage>
    {
        private readonly IEmailNotifier _emailNotifier;

        public EmailNotificationWorker(
            IEmailNotifier emailNotifier,
            IEventRepository eventRepository,
            IServiceBus serviceBus)
            : base(eventRepository, serviceBus)
        {
            _emailNotifier = emailNotifier;
        }

        public override void ProcessMessage(EmailCommandMessage message)
        {
            _emailNotifier.SendEmail(
                message.FromEmail,
                message.FromName,
                message.ToEmail,
                message.ToName,
                message.Subject,
                message.Body,
                FinalizeMessage);
        }
    }
}