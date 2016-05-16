using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using AFT.RegoV2.Core.Common.Events.Notifications;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Infrastructure.Mail
{
    public class EmailNotifier : IEmailNotifier
    {
        private readonly SmtpClient _smtpClient;

        public EmailNotifier()
        {
            var host = ConfigurationManager.AppSettings["Smtp.Host"];
            var port = int.Parse(ConfigurationManager.AppSettings["Smtp.Port"]);
            var userName = ConfigurationManager.AppSettings["Smtp.UserName"];
            var password = ConfigurationManager.AppSettings["Smtp.Password"];

            _smtpClient = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = true
            };
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
            var notificationSentEvent = new NotificationSentEvent
            {
                Status = NotificationStatus.Send,
                Type = NotificationType.Email,
                Reciever = toEmail,
                Subject = subject,
                Message = body
            };

            if (!bool.Parse(ConfigurationManager.AppSettings["EnableEmails"]))
            {
                callback(notificationSentEvent);
                return;
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };

            mailMessage.To.Add(new MailAddress(toEmail, toName));

            _smtpClient.SendCompleted += (sender, args) =>
            {
                if (args.Error != null)
                {
                    notificationSentEvent.Status = NotificationStatus.Error;
                    notificationSentEvent.Error = args.Error.Message;
                }

                callback(notificationSentEvent);
            };

            _smtpClient.SendAsync(mailMessage, null);
        }
    }
}