using System;
using System.Security.Principal;

namespace AFT.RegoV2.Infrastructure.OAuth2
{

    public interface IPlayerPrincipal : IPrincipal
    {
        Guid Id { get; set; }
    }

    public class PlayerPrincipal :IPlayerPrincipal
    {
        public Guid Id { get; set; }

        public IIdentity Identity { get; private set; }

        public bool IsInRole(string role)
        {
            return false;
        }

        public PlayerPrincipal(string name)
        {
            Identity = new GenericIdentity(name);
        }
    }
}
