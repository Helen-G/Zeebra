using System;
using System.Web;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Infrastructure.DependencyResolution
{
    public class PerHttpRequestLifetime : LifetimeManager
    {
        private readonly Guid _key = Guid.NewGuid();

        public override object GetValue()
        {
            var context = HttpContext.Current;
            return context == null ? null : context.Items[_key];
        }

        public override void SetValue(object newValue)
        {
            var context = HttpContext.Current;
            if (context != null)
            {
                context.Items[_key] = newValue;
            }
        }

        public override void RemoveValue()
        {
            var context = HttpContext.Current;
            if (context == null) return;
            var obj = GetValue();
            context.Items.Remove(obj);
        }
    }
}