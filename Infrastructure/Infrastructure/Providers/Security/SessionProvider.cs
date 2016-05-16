using System.Web;
using System.Web.SessionState;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.Security.Data;
using AFT.RegoV2.Infrastructure.Providers.Security.Base;

namespace AFT.RegoV2.Infrastructure.Providers.Security
{
    public class SessionProvider : SessionProviderBase
    {
        private static HttpSessionState Session
        {
            get { return HttpContext.Current.Session; }
        }

        protected override void Set<T>(string key, T value)
        {
            Session[key] = value;
        }

        protected override T Get<T>(string key)
        {
            return (T)Session[key];
        }

    }
}
