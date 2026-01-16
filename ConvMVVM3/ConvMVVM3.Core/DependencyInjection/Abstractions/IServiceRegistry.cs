using System;

namespace ConvMVVM3.Core.DependencyInjection.Abstractions
{

    public interface IServiceRegistry
    {

        #region Public Functions
        void Add(Type serviceType, Type implementationType, ServiceLifetime lifetime);
        void AddFactory(Type serviceType, Func<IServiceResolver, object> factory, ServiceLifetime lifetime);

        // 제네릭 편의
        void AddSingleton<TService, TImpl>() where TImpl : TService;
        void AddScoped<TService, TImpl>() where TImpl : TService;
        void AddTransient<TService, TImpl>() where TImpl : TService;

        void AddSingleton<TService>(Func<IServiceResolver, TService> factory);
        void AddScoped<TService>(Func<IServiceResolver, TService> factory);
        void AddTransient<TService>(Func<IServiceResolver, TService> factory);
        #endregion
    }






}
