using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Commands
{
    public class EmailCommandMessage : ICommand
    {
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string ToEmail { get; set; }
        public string ToName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public EmailCommandMessage(
            string fromEmail,
            string fromName,
            string toEmail,
            string toName,
            string subject,
            string body)
        {
            FromEmail = fromEmail;
            FromName = fromName;
            ToEmail = toEmail;
            ToName = toName;
            Subject = subject;
            Body = body;
        }

        public EmailCommandMessage()
        {
        }
    }
}