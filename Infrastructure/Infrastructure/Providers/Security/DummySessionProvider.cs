using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Infrastructure.Providers.Security.Base;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Infrastructure.Providers.Security
{
    public class DummySessionProvider : SessionProviderBase
    {
        protected override void Set<T>(string key, T value)
        {
            throw new RegoException("Session is not supported");
        }

        protected override T Get<T>(string key)
        {
            throw new RegoException("Session is not supported");
        }
    }
}
