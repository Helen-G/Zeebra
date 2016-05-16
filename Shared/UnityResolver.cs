using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Microsoft.Practices.Unity;

namespace AFT.RegoV2.Shared
{
    public sealed class UnityResolver : IDependencyResolver
    {
        private readonly IUnityContainer _container;
        private readonly HttpConfiguration _config;

        public UnityResolver(IUnityContainer container, HttpConfiguration config)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            _container = container;
            _config = config;

        }      

        object IDependencyScope.GetService(Type serviceType)
        {
            try
            {
                return _container.Resolve(serviceType);
            }
            catch (ResolutionFailedException rfe)
            {
                if (serviceType.Namespace.StartsWith("System") == false)
                {
                    Debug.WriteLine("Unable to resolve " + serviceType.Namespace + "." + serviceType.Name);

                    if (rfe.InnerException != null)
                    {
                        Debug.WriteLine("*** " + rfe.InnerException.Message);
                    }
                }
                return null;
            }
        }

        IEnumerable<object> IDependencyScope.GetServices(Type serviceType)
        {
            try
            {
                return _container.ResolveAll(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return new List<object>();
            }
        }

        IDependencyScope IDependencyResolver.BeginScope()
        {
            try
            {
                var child = _container.CreateChildContainer();
                return new UnityResolver(child, _config);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("{0}: BeginScope::" + ex, Guid.NewGuid().ToString());
                return null;
            }
        }

        void IDisposable.Dispose()
        {
            if (_container != null)
            {
                _container.Dispose();
            }
        }
    }
}