using System;
using TechTalk.SpecFlow;

namespace AFT.RegoV2.GameApi.Tests.Core
{
    public abstract class SpecFlowUnitTestBase : MoqUnitTestBase
    {
        protected static ScenarioContext Context { get { return ScenarioContext.Current; } }

        protected static T Get<T>(string key, T defaultValue = default(T))
        {
            return Context.ContainsKey(key)
                    ? Context.Get<T>(key)
                    : defaultValue;
        }
        protected static T GetOrCreate<T>(string key, Func<T> create)
        {
            return Context.ContainsKey(key)
                    ? Context.Get<T>(key)
                    : (T)(Context[key] = create());
        }
        protected static T Set<T>(string key, T value)
        {
            Context[key] = value;
            return value;
        }
    }

}