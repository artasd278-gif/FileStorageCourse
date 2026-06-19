using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace FileStorageMVC.App_Start
{
    public class SimpleDependencyResolver : IDependencyResolver
    {
        private readonly IDictionary<Type, Func<object>> _registrations = new Dictionary<Type, Func<object>>();

        public void Register<TService>(Func<object> factory)
        {
            _registrations[typeof(TService)] = factory;
        }

        public object GetService(Type serviceType)
        {
            Func<object> factory;
            if (_registrations.TryGetValue(serviceType, out factory))
            {
                return factory();
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            var service = GetService(serviceType);
            if (service == null)
            {
                return new object[0];
            }

            return new[] { service };
        }
    }
}
