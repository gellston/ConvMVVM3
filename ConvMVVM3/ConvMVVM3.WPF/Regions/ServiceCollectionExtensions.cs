using System;
using ConvMVVM3.Core.DependencyInjection.Abstractions;
using ConvMVVM3.WPF.Abstractions;
using ConvMVVM3.WPF.Regions.Adapter;


namespace ConvMVVM3.WPF.Regions
{
    /// <summary>
    /// DI registration helpers.
    /// Registers default region adapters and a singleton RegionAdapterManager built from the registered adapters.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceRegistry AddConvMVVM3Regions(this IServiceRegistry services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // Default adapters
            services.AddSingleton<IRegionAdapter, SelectorRegionAdapter>();
            services.AddSingleton<IRegionAdapter, ContentControlRegionAdapter>();
            services.AddSingleton<IRegionAdapter, ItemsControlRegionAdapter>();
            services.AddSingleton<IRegionAdapter, PanelRegionAdapter>();

            services.AddSingleton<RegionAdapterManager>((IServiceResolver sp) =>
            {
                var adapters = sp.GetServices<IRegionAdapter>();
                return new RegionAdapterManager(adapters);
            });

            return services;
        }
    }
}
