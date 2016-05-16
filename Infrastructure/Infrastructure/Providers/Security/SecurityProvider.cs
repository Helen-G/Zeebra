using System;
using System.Configuration;
using System.Web;
using AFT.RegoV2.Core.Security.Interfaces;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Domain.Security.Data;
using AFT.RegoV2.Domain.Security.Interfaces;

namespace AFT.RegoV2.Infrastructure.DataAccess.Security.Providers
{
    public class SecurityProvider : ISecurityProvider, IDisposable
    {
        private readonly ISessionProvider _session;

        public SecurityProvider(ISessionProvider session)
        {
            _session = session;
        }

        public ISessionProvider Session 
        {
            get { return _session; }
        }

        public AuthUser User
        {
            get { return _session.User; }
        }

        public bool IsUserAvailable
        {
            get { return _session.User != null; }
        }

        public void SetUser(User user)
        {
            _session.SetUser(user);
        }

        public void ClearUser()
        {
            _session.ClearUser();
        }

        public void Dispose()
        {
            ClearUser();
        }
    }
}
