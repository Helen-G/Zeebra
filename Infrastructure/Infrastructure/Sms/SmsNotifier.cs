using System;
using System.Configuration;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Infrastructure.Sms
{
    public class SmsNotifier : ISmsNotifier
    {
        public virtual void SendSms(string phoneNumber, string body, Action<NotificationSentEvent> callback = null)
        {
            var notificationEvent = new NotificationSentEvent
            {
                Status = NotificationStatus.Send,
                Type = NotificationType.Sms,
                Message = body,
                Reciever = phoneNumber
            };

            var isInReleaseMode = bool.Parse(ConfigurationManager.AppSettings["ProductionMode"]);
            if (isInReleaseMode)
            {
                var r = SmsProxy.Send(phoneNumber, body);
                if (r.Code != "0" || r.Code != "1")
                {
                    notificationEvent.Error = r.Description;
                    notificationEvent.Status = NotificationStatus.Error;
                }
            }
            
            if (callback != null)
                callback(notificationEvent);
        }
    }
}