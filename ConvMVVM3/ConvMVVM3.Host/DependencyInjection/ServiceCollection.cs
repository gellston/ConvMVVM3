using ConvMVVM3.Core.DependencyInjection.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConvMVVM3.Host.DependencyInjection
{
    public sealed class ServiceCollection : IServiceRegistry
    {
        #region Private Property
        private readonly List<ServiceDescriptor> _descriptors = new List<ServiceDescriptor>();
        #endregion


        #region Public Property

        public IReadOnlyList<ServiceDescriptor> Descriptors
        {
            get { return _descriptors; }
        }
        #endregion


        #region Public Functions

        public void Add(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));

            _descriptors.Add(new ServiceDescriptor
            {
                ServiceType = serviceType,
                ImplementationType = implementationType,
                Lifetime = lifetime
            });
        }

        public void AddFactory(Type serviceType, Func<IServiceResolver, object> factory, ServiceLifetime lifetime)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            _descriptors.Add(new ServiceDescriptor
            {
                ServiceType = serviceType,
                Factory = factory,
                Lifetime = lifetime
            });
        }

        public void AddSingleton<TService, TImpl>() where TImpl : TService
            => Add(typeof(TService), typeof(TImpl), ServiceLifetime.Singleton);

        public void AddScoped<TService, TImpl>() where TImpl : TService
            => Add(typeof(TService), typeof(TImpl), ServiceLifetime.Scoped);

        public void AddTransient<TService, TImpl>() where TImpl : TService
            => Add(typeof(TService), typeof(TImpl), ServiceLifetime.Transient);

        public void AddSingleton<TService>(Func<IServiceResolver, TService> factory)
            => AddFactory(typeof(TService), sp => (object)factory(sp), ServiceLifetime.Singleton);

        public void AddScoped<TService>(Func<IServiceResolver, TService> factory)
            => AddFactory(typeof(TService), sp => (object)factory(sp), ServiceLifetime.Scoped);

        public void AddTransient<TService>(Func<IServiceResolver, TService> factory)
            => AddFactory(typeof(TService), sp => (object)factory(sp), ServiceLifetime.Transient);

        #endregion
    }
}
