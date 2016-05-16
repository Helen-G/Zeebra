using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Events.Notifications
{
    public class NotificationSentEvent : DomainEventBase
    {
        public NotificationStatus Status { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public string Reciever { get; set; }
        public string Subject { get; set; }
    }
}