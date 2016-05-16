using System;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeEmailNotifier : IEmailNotifier
    {
        public void SendEmail(string brand, string to, string subject, string emailBody, Action<NotificationSentEvent> callback = null)
        {
            if (callback == null) return;

            callback(new NotificationSentEvent
            {
                Message = emailBody,
                Reciever = to,
                Status = NotificationStatus.Send,
                Type = NotificationType.Email,
                Subject = subject
            });   
        }

        public void SendEmail(
            string fromEmail, 
            string fromName, 
            string toEmail, 
            string toName, 
            string subject, 
            string body,
            Action<NotificationSentEvent> callback)
        {
            callback(new NotificationSentEvent
            {
                Status = NotificationStatus.Send,
                Type = NotificationType.Email,
                Reciever = toEmail,
                Subject = subject,
                Message = body
            });
        }
    }
}
