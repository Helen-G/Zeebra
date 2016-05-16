using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Common.Commands
{
    public class SmsCommandMessage : ICommand
    {
        public string PhoneNumber { get; private set; }
        public string Body { get; private set; }

        public SmsCommandMessage(string phoneNumber, string body)
        {
            PhoneNumber = phoneNumber;
            Body = body;
        }

        public SmsCommandMessage()
        {
            
        }
    }
}