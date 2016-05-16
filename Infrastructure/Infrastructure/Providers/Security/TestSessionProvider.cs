using System.Collections.Generic;
using AFT.RegoV2.Infrastructure.Providers.Security.Base;

namespace AFT.RegoV2.Infrastructure.Providers.Security
{
    public class TestSessionProvider : SessionProviderBase
    {
        private readonly Dictionary<string, object> _testSession;

        public TestSessionProvider()
        {
            _testSession = new Dictionary<string, object>();
        }

        protected override void Set<T>(string key, T value)
        {
            if (_testSession.ContainsKey(key))
                _testSession[key] = value;
            else
                _testSession.Add(key, value);
        }

        protected override T Get<T>(string key)
        {
            object result;
            _testSession.TryGetValue(key, out result);
            return (T)result;
        }
    }
}
