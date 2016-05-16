using System;
using AFT.RegoV2.Core.Common.Events.Notifications;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IEmailNotifier
    {
        void SendEmail(
            string fromEmail,
            string fromName,
            string toEmail,
            string toName,
            string subject,
            string body,
            Action<NotificationSentEvent> callback);
    }
}