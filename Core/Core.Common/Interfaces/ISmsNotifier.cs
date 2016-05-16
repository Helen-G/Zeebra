using System;
using AFT.RegoV2.Core.Common.Events.Notifications;

namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface ISmsNotifier
    {
        void SendSms(
            string phoneNumber, 
            string body, 
            Action<NotificationSentEvent> callback = null);
    }
}