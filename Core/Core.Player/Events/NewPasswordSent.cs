using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class NewPasswordSent : DomainEventBase
    {
        public string UserName { get; set; }
        public string Email { get; set; }

        public NewPasswordSent()
        {
            
        }

        public NewPasswordSent(string userName, string email)
        {
            UserName = userName;
            Email = email;
        }
    }
}
