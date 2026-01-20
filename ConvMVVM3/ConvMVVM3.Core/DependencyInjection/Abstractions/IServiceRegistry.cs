using System;

namespace ConvMVVM3.Core.DependencyInjection.Abstractions
{

    public interface IServiceRegistry
    {

        #region Public Functions
        void Add(Type serviceType, Type implementationType, string key, ServiceLifetime lifetime);
        void AddFactory(Type serviceType, string key, Func<IServiceResolver, object> factory, ServiceLifetime lifetime);

        // 제네릭 편의
        void AddSingleton<TService, TImpl>() where TImpl : TService;
        void AddScoped<TService, TImpl>() where TImpl : TService;
        void AddTransient<TService, TImpl>() where TImpl : TService;

        void AddSingleton<TService>(Func<IServiceResolver, TService> factory);
        void AddScoped<TService>(Func<IServiceResolver, TService> factory);
        void AddTransient<TService>(Func<IServiceResolver, TService> factory);




        void AddSingleton<TService, TImpl>(string key) where TImpl : TService;
        void AddScoped<TService, TImpl>(string key) where TImpl : TService;
        void AddTransient<TService, TImpl>(string key) where TImpl : TService;

        void AddSingleton<TService>(string key, Func<IServiceResolver, TService> factory);
        void AddScoped<TService>(string key, Func<IServiceResolver, TService> factory);
        void AddTransient<TService>(string key, Func<IServiceResolver, TService> factory);
        #endregion
    }






}
